using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using OmiLAXR.Components;
using OmiLAXR.Endpoints;
using OmiLAXR.Pipelines;
using OmiLAXR.ReCoPa.Filters;
using OmiLAXR.ReCoPa.Network;
using OmiLAXR.Types;
using OmiLAXR.xAPI;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace OmiLAXR.ReCoPa
{
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa")]
    [DefaultExecutionOrder(-1)]
    public class ReCoPa : PipelineComponent, IDebugSender
    {
        public string connectionUrl = "http://127.0.0.1:4567";

        // ✅ Unity main-thread context
        private SynchronizationContext _unityCtx;

        private void RunOnUnityThread(Action a)
        {
            // Unity's SynchronizationContext executes these on main thread
            _unityCtx.Post(_ => a(), null);
        }

        // TCP Socket client
        private UnitySocketClient _socket;

        [SerializeField] private Pipeline targetPipeline;

        private Coroutine _scenarioUpdateCoroutine;
        private bool _wasTracking;

        public xApiRegistry xApiRegistry;
        [SerializeField] private List<Endpoint> endpoints;

        public bool IsConnected => _socket != null && _socket.Connected;
        public UnityEvent onConnected = new UnityEvent();
        public UnityEvent onDisconnected = new UnityEvent();
        public UnityEvent onReconnected = new UnityEvent();

        private bool _isTrackingPaused;
        private TrackingScenario? _currentScenario;
        private TrackingConfig? _trackingConfig;
        private readonly List<string> _gameObjects = new();
        private string[] _actions;
        private string[] _gestures;

        private ICalibratable _eyeTrackingModule;
        private ReCoPaFilter _filter;

        private string sceneName => SceneManager.GetActiveScene().name;
        private bool _isDirty;

        public bool doReconnection = true;
        public int reconnectionDelay = 30_000;
        public int reconnectionMaxDelay = 60_000;
        public int reconnectionAttempts = 10;

        public TrackingMeta GetMeta(string metaContext) => new TrackingMeta()
        {
            //isTracking = targetPipeline.IsRunning,
            isTrackingPaused = _isTrackingPaused,
            //isCalibrated = _eyeTrackingModule?.IsCalibrated ?? false,
            computerName = Environment.MachineName,
            actorName = targetPipeline.actor.actorName,
            actorEmail = targetPipeline.actor.actorEmail,
            metaContext = metaContext,
        };

        private void Awake()
        {
            // ✅ capture Unity main thread context
            _unityCtx = SynchronizationContext.Current;
            if (_unityCtx == null)
            {
                // Very rare in Unity, but fail loudly so you notice
                Debug.Error("[ReCoPa] SynchronizationContext is null. Main-thread dispatch will not work.");
                _unityCtx = new SynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(_unityCtx);
            }

#if UNITY_2021_1_OR_NEWER
            targetPipeline = FindFirstObjectByType<LearnerPipeline>();
            xApiRegistry = FindFirstObjectByType<xApiRegistry>();
#else
            targetPipeline = FindObjectOfType<LearnerPipeline>();
            xApiRegistry = FindObjectOfType<xApiRegistry>();
#endif
            _filter = GetComponentInChildren<ReCoPaFilter>();
            targetPipeline.Add(_filter);

            _eyeTrackingModule = targetPipeline.GetComponentInChildren<ICalibratable>();

            Init();
            InitSocket();
        }

        private void Init()
        {
            if (_eyeTrackingModule != null)
            {
                _eyeTrackingModule.OnCalibrationStarted += () => SendMeta("calibration:start");
                _eyeTrackingModule.OnCalibrationEnded += _ => SendMeta("calibration:stop");
            }

            if (!targetPipeline)
            {
                DebugLog.Warning("Cannot find a <LearnerPipeline>.");
                return;
            }

            targetPipeline.AfterFoundObjects += objects =>
            {
                if (objects == null) return;
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
            
            _ = _socket.EmitAsync("clients:tracking", JObject.FromObject(config).ToString());
        }

        private void StartTracking()
        {
            foreach (var e in endpoints) e.StartSending();
            targetPipeline.StartPipeline();
        }

        private void PauseTracking()
        {
            foreach (var e in endpoints) e.PauseSending();
            _isTrackingPaused = true;
        }

        private void ResumeTracking()
        {
            foreach (var e in endpoints) e.StartSending();
            _isTrackingPaused = false;
        }

        private void StopTracking()
        {
            foreach (var e in endpoints) e.StopSending();
            targetPipeline.StopPipeline();
        }

        private void InitSocket()
        {
            if (_socket != null) return;

            _socket = new UnitySocketClient(connectionUrl, new SocketClientOptions()
            {
                Reconnection = doReconnection,
                ReconnectionDelay = reconnectionDelay,
                ReconnectionDelayMax = reconnectionMaxDelay,
                ReconnectionAttempts = reconnectionAttempts,
                ExtraHeaders = new Dictionary<string, string>()
                {
                    ["clientType"] = "participant",
                    ["version"] = "2.0.0"
                }
            }, UnitySocketClient.UnityThreadScope.Update);

            _socket.OnConnected += (_, __) => OnConnected();
            _socket.OnReconnected += (_, __) => OnReconnected();
            _socket.OnDisconnected += (_, __) => OnDisconnected();

            _socket.OnReconnectAttempt += (_, i) => DebugLog.Warning("Reconnecting to ReCoPa... Attempt " + i);
            _socket.OnReconnectError += (_, ex) => DebugLog.Error($"Reconnection error '{ex}'.");
            _socket.OnReconnectFailed += (_, __) =>
            {
                DebugLog.Error("Failed connecting to ReCoPa. Make sure you have started it.");
                enabled = false;
            };

            _socket.OnError += (_, msg) => DebugLog.Error($"Error '{msg}'.");

            _socket.On("clients:quit", _ => RunOnUnityThread(Quit));
            _socket.On("clients:all", _ => _isDirty = true);

            _socket.On("clients:scenario", DispatchScenarioInformation);

            _socket.On("clients:calibration:start", _ => _eyeTrackingModule.StartCalibration());
            _socket.On("clients:calibration:stop", _ => _eyeTrackingModule.StopCalibration());

            _socket.On("clients:tracking", DispatchTrackingInformation);
            _socket.On("clients:tracking:start", DispatchStartTracking);
            _socket.On("clients:tracking:stop", DispatchStopTracking);
            _socket.On("clients:tracking:pause", DispatchPauseTracking);
            _socket.On("clients:tracking:resume", DispatchResumeTracking);

            _ = _socket.ConnectAsync();
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

            if (_scenarioUpdateCoroutine != null)
                StopCoroutine(_scenarioUpdateCoroutine);
        }

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
            if (_socket == null) return;
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }

        /// <summary>
        /// Sends meta information to socket server.
        /// </summary>
        private void SendMeta(string metaContext)
        {
            if (_socket == null) return;

            // already on unity thread typically, but safe:
            RunOnUnityThread(() =>
            {
                _ = _socket.EmitAsync("clients:meta", GetMeta(metaContext));
            });
        }

        private IEnumerator UpdateScenario()
        {
            while (true)
            {
                if (_socket.Connected && _isDirty)
                    SendScenario();

                yield return new WaitForSeconds(5);
            }
        }

        /// <summary>
        /// Sends all gameObjects and actions to socket server.
        /// </summary>
        private void SendScenario(bool reload = false)
        {
            if (_socket == null) return;

            var scenario = GetScenario(reload);
            var tracking = GetTrackingConfig(scenario);

            _ = _socket.EmitAsync("clients:scenario", scenario);
            _ = _socket.EmitAsync("clients:tracking", tracking);

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

        private void DispatchStartTracking(SocketResponse e)
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
        }

        private void DispatchPauseTracking(SocketResponse e) => PauseTracking();
        private void DispatchResumeTracking(SocketResponse e) => ResumeTracking();
        private void DispatchStopTracking(SocketResponse e) => StopTracking();

        private void DispatchTrackingInformation(SocketResponse e)
        {
            var tracking = GetScenarioTrackingConfig();
            _ = _socket.EmitAsync("clients:tracking", JObject.FromObject(tracking));
        }

        private void DispatchScenarioInformation(SocketResponse e)
        {
            var scenario = GetScenario();
            _ = _socket.EmitAsync("clients:scenario", JObject.FromObject(scenario));
        }
        
        public TrackingScenario GetScenario(bool reload = false)
        {
            if (!reload && _currentScenario.HasValue) return _currentScenario.Value;

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