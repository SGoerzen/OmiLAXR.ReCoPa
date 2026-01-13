using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OmiLAXR.Components;
using OmiLAXR.Endpoints;
using OmiLAXR.Pipelines;
using OmiLAXR.ReCoPa.Filters;
using OmiLAXR.Types;
using OmiLAXR.Utils;
using OmiLAXR.xAPI;

using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using SocketIOClient.Transport;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace OmiLAXR.ReCoPa
{
    /// <summary>
    /// This adapter is needed to connect OmiLAXR Tracking System with the Researcher Companion Panel.
    /// </summary>
    [RequireComponent(typeof(UnityMainThreadDispatcher))]
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa")]
    [DefaultExecutionOrder(-1)]
    public class ReCoPa : PipelineComponent, IDebugSender
    {
        private UnityMainThreadDispatcher _dispatcher;
        public string connectionUrl = "http://127.0.0.1:4567";

        // variables for websocket communication
        private SocketIOUnity _socket;

        [SerializeField, Tooltip("Target pipeline ReCoPa is communicating with. By default (if empty) it will look for <LearnerPipeline>.")]
        private Pipeline targetPipeline;

        private Coroutine _scenarioUpdateCoroutine;
        private bool _wasTracking;

        public xApiRegistry xApiRegistry;
        
        [SerializeField]
        private List<Endpoint> endpoints;

        public bool IsConnected => _socket != null && _socket.Connected;
        public UnityEvent onConnected = new UnityEvent();
        public UnityEvent onDisconnected = new UnityEvent();
        public UnityEvent onReconnected = new UnityEvent();
        
        private bool _isTrackingPaused;

        private TrackingScenario? _currentScenario;
        private TrackingConfig? _trackingConfig;
        private List<string> _gameObjects = new List<string>();
        private string[] _actions;
        private string[] _gestures;

        private ICalibratable _eyeTrackingModule;
        private ReCoPaFilter _filter;
        
        private string sceneName => SceneManager.GetActiveScene().name;

        private bool _isDirty = false;

        public bool doReconnection = true;
        public int reconnectionDelay = 30_000;
        public int reconnectionMaxDelay = 60_000;
        public int reconnectionAttempts = 10;

        public TrackingMeta GetMeta(string metaContext) => new TrackingMeta()
        {
            ["isTracking"] = targetPipeline.IsRunning,
            ["isTrackingPaused"] = _isTrackingPaused,
            ["isCalibrated"] = _eyeTrackingModule?.IsCalibrated,
            ["computerName"] = Environment.MachineName,
            ["actorName"] = targetPipeline.actor.actorName,
            ["actorEmail"] = targetPipeline.actor.actorEmail,
            ["metaContext"] = metaContext,
        };
        
        private void Awake()
        {
#if UNITY_2021_1_OR_NEWER
            targetPipeline = FindFirstObjectByType<LearnerPipeline>();
#else
            targetPipeline = FindObjectOfType<LearnerPipeline>();
            _learningRecordStore = FindObjectOfType<LearningRecordStore>();
#endif
            _filter = GetComponentInChildren<ReCoPaFilter>();
            targetPipeline.Add(_filter);
            
            _eyeTrackingModule = targetPipeline.GetComponentInChildren<ICalibratable>();
            
            Init();
            InitSocket();
            
            //SceneManager.sceneLoaded += ChangedScene;
        }

        private void Init()
        {
            if (_eyeTrackingModule != null)
            {
                _eyeTrackingModule.OnCalibrationStarted += () => SendMeta("calibration:start");
                _eyeTrackingModule.OnCalibrationEnded += (successful) => SendMeta("calibration:stop");
            }
            else
            {
                DebugLog.Warning("Cannot find any eye tracking module.");
            }

            var p = targetPipeline;
            if (!targetPipeline)
            {
                DebugLog.Warning("Cannot find a <LearnerPipeline>.");
                return;
            }

            p.AfterFoundObjects += objects =>
            {
                if (objects == null)
                    return;
                _gameObjects.AddRange(objects.Select(o => o.GetTrackingName()));
                _gameObjects.Sort();
            };
            targetPipeline.AfterStartedPipeline += HookIntoLearner;
        }

        private void HookIntoLearner(Pipeline p)
        {
            p.AfterStartedPipeline -= HookIntoLearner;
            
            _actions = p.Actions.Keys.ToArray();
            Array.Sort(_actions);
                
            _gestures = p.Gestures.Keys.ToArray();
            Array.Sort(_gestures);
                
            var config = new TrackingConfig()
            {
                gameObjects = _gameObjects.ToArray(),
                gestures = _gestures,
                actions = _actions
            };
            
            StopTracking();
            _filter.enabled = true;
            
            p.AfterStartedPipeline += (_) =>
            {
                _wasTracking = true;
                SendMeta("tracking:start");
            };
            
            p.BeforeStoppedPipeline += (_) =>
            {
                _wasTracking = false;
                SendMeta("tracking:stop");
            };
            
            _socket.Emit("clients:tracking", JObject.FromObject(config));
        }
        
        private void StartTracking()
        {
            //_learningRecordStore.StartSending();
            targetPipeline.StartPipeline();
        }

        private void PauseTracking()
        {
            _isTrackingPaused = true;
        }

        private void ResumeTracking()
        {
            _isTrackingPaused = false;
        }

        private void StopTracking()
        {
            //_learningRecordStore.StopSending();
            targetPipeline.StopPipeline();
        }
        
        private void InitSocket()
        {
            // stop if a socket is assigned
            if (_socket != null)
                return;

            _socket = new SocketIOUnity(connectionUrl, new SocketIOOptions()
            {
                Transport = TransportProtocol.WebSocket,
                Reconnection = doReconnection,
                ReconnectionDelay = reconnectionDelay,
                ReconnectionDelayMax = reconnectionMaxDelay,
                ReconnectionAttempts = reconnectionAttempts,
                ExtraHeaders = new Dictionary<string, string>()
                {
                    ["clientType"] = "participant",
                    ["version"] = "2.0.0"
                }
            }, SocketIOUnity.UnityThreadScope.FixedUpdate);
            _socket.JsonSerializer = new NewtonsoftJsonSerializer();

            // Initialize socket.io communication events
            _socket.OnConnected += (_, _) => OnConnected();
            _socket.OnReconnected += (_, _) => OnReconnected();
            _socket.OnDisconnected += (_, _) => OnDisconnected();
            _socket.OnReconnectAttempt += (_, i) =>
            {
                DebugLog.Warning("Reconnecting to ReCoPa... Attempt " + i);
            };
            _socket.OnReconnectError += (_, exception) =>
            {
                DebugLog.Error($"Reconnection error '{exception}'.");
            };
            _socket.OnReconnectFailed += (_, args) =>
            {
                DebugLog.Error("Failed connecting to ReCoPa. Make sure you have started it.");
                enabled = false;
            };
            _socket.OnError += (_, msg) => { DebugLog.Error($"Error '{msg}'."); };

            _socket.OnUnityThread("clients:quit", _ => Quit());

            _socket.On("clients:all", _ => _isDirty = true);

            _socket.On("clients:scenario", DispatchScenarioInformation);

             _socket.On("clients:calibration:start", _ => _eyeTrackingModule.StartCalibration());
             _socket.On("clients:calibration:stop", _ => _eyeTrackingModule.StopCalibration());

            _socket.On("clients:tracking", DispatchTrackingInformation);
            _socket.On("clients:tracking:start", DispatchStartTracking);
            _socket.On("clients:tracking:stop", DispatchStopTracking);
            _socket.On("clients:tracking:pause", DispatchPauseTracking);
            _socket.On("clients:tracking:resume", DispatchResumeTracking);

            _socket.ConnectAsync();
        }

        private void OnConnected()
        {
            // Send meta information first time
            DebugLog.Print("Connected to ReCoPa.");
            onConnected.Invoke();
            SendMeta("connected");
            BeginScenarioUpdate();
        }

        private void OnReconnected()
        {
            DebugLog.Print("Reconnected to ReCoPa.");
            onReconnected.Invoke();
            SendMeta("reconnected");
            BeginScenarioUpdate();
        }

        private void BeginScenarioUpdate()
        {
            _isDirty = true;
            _scenarioUpdateCoroutine = StartCoroutine(UpdateScenario());
        }

        private void OnDisconnected()
        {
            DebugLog.Print("Disconnected from ReCoPa.");
            onDisconnected.Invoke();
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                if (_scenarioUpdateCoroutine != null)
                    StopCoroutine(_scenarioUpdateCoroutine);
            });
        }

        /// <summary>
        /// Close application.
        /// </summary>
        private static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnApplicationQuit()
        {
            if (_socket == null)
                return;
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }

        /// <summary>
        /// Sends meta information to socket server.
        /// </summary>
        private void SendMeta(string metaContext)
        {
            if (_socket == null)
                return;
        
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                _socket.EmitAsync("clients:meta", GetMeta(metaContext));
            });
        }
        
        private IEnumerator UpdateScenario()
        {
            while (true)
            {
                if (_socket.Connected && _isDirty)
                {
                    SendScenario();
                }
        
                yield return new WaitForSeconds(5);
            }
        }

        /// <summary>
        /// Sends all gameObjects and actions to socket server.
        /// </summary>
        private void SendScenario(bool reload = false)
        {
            if (_socket == null)
                return;
            
            var scenario = GetScenario(reload);
            var tracking = GetTrackingConfig(scenario);

            UnityEngine.Debug.Log(scenario);
            UnityEngine.Debug.Log(tracking);
            
            // transfer resulted JSONObject to socket server
            _socket.EmitAsync("clients:scenario", scenario);
            _socket.EmitAsync("clients:tracking", tracking);
            
            DebugLog.Print("Sent scenario information.");
            
            _isDirty = false;
        }
        public TrackingConfig GetScenarioTrackingConfig() => GetTrackingConfig(GetScenario());
        public TrackingConfig GetTrackingConfig(TrackingScenario scenario)
        {
            if (_trackingConfig.HasValue) 
                return _trackingConfig.Value;

            var lrs = FindAnyObjectByType<OmiLAXR.xAPI.Endpoints.LearningRecordStore>();
            var actor = targetPipeline.actor;
            
            if (xApiRegistry == null)
                xApiRegistry = FindFirstObjectByType<xApiRegistry>();
            
            var uri = xApiRegistry.uri;

            var credentials = lrs.Credentials;
            var endpoint = credentials.endpoint;
            var key = credentials.username;
            var secret = credentials.password;
            var actorName = actor.actorName;
            var actorEmail = actor.actorEmail;
            
            if(!actorEmail.StartsWith("mailto:"))
                actorEmail = "mailto:" + actorEmail;
            
            _trackingConfig = new TrackingConfig()
            {
                auth = new TrackingConfig.ClientAuth(key, secret),
                lrs = endpoint,
                uri = uri,
                identity = new TrackingConfig.TrackingIdentity(actorName, actorEmail),
                gameObjects = scenario.gameObjects,
                actions = scenario.actions,
                gestures = scenario.gestures
            };

            return _trackingConfig.Value;
        }

        private void SetupEndpoints(EndpointConfigs map)
        {
            // setup endpoint configs
            var endpoints = FindObjectsByType<Endpoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var endpoint in endpoints)
            {
                var epName = endpoint.GetType().Name;
                var config = map[epName];
                if (config == null)
                    continue;
                endpoint.ConsumeDataMap(config);
            }
        }

        /// <summary>
        /// Serialize e.data to TrackingInfo, call Setup and Start Tracking.
        /// </summary>
        /// <param name="e">e.data contains {username, email, lrs, uri, authUsername, authKey }</param>
        private void DispatchStartTracking(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                var config = e.GetValue<TrackingConfig>();

                foreach (var endpoint in endpoints)
                {
                    //endpoint.ConsumeDataMap(config);
                }

                if (_trackingConfig.HasValue)
                    SetupEndpoints(_trackingConfig.Value.endpoints);

                if (xApiRegistry == null)
                    xApiRegistry = FindFirstObjectByType<xApiRegistry>();
                
                xApiRegistry.uri = config.uri;

                // apply game objects filter
                _filter.gameObjects = config.gameObjects;
                
                // disable all actions
                targetPipeline.SetDisabledActions(true);
                // enable only selected actions
                targetPipeline.SetDisabledActions(false, config.actions);
                
                // disable all gestures
                targetPipeline.SetDisabledGestures(true);
                // enable only selected gestures
                targetPipeline.SetDisabledGestures(false, config.gestures);
                
                StartTracking();
            });
        }
        
        private void DispatchPauseTracking(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(PauseTracking);
        }
        
        private void DispatchResumeTracking(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(ResumeTracking);
        }

        private void DispatchStopTracking(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(StopTracking);
        }

        private void DispatchTrackingInformation(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                var tracking = GetScenarioTrackingConfig();
                _socket.EmitAsync("clients:tracking", JObject.FromObject(tracking));
            });
        }

        private void DispatchScenarioInformation(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                var scenario = GetScenario();
                _socket.EmitAsync("clients:scenario", JObject.FromObject(scenario));
            });
        }
        
        public TrackingScenario GetScenario(bool reload = false)
        {
            if (!reload && _currentScenario.HasValue)
                return _currentScenario.Value;
            
            _currentScenario = new TrackingScenario()
            {
                name = sceneName,
                gameObjects = _gameObjects.ToArray(),
                actions = _actions,
                gestures = _gestures
            };
            return _currentScenario.Value;
        }

        private static readonly DebugLog Debug = new DebugLog("ReCoPa Module");
        public DebugLog DebugLog => Debug;
    }

}