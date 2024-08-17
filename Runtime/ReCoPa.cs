using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OmiLAXR.Pipelines;
using OmiLAXR.xAPI.Endpoints;
using PimDeWitte.UnityMainThreadDispatcher;

using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using SocketIOClient.Transport;

using UnityEditor;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace OmiLAXR.Modules.ReCoPa
{
    /// <summary>
    /// This adapter is needed to connect OmiLAXR Tracking System with the Researcher Companion Panel.
    /// </summary>
    [RequireComponent(typeof(SocketIOUnity), typeof(UnityMainThreadDispatcher))]
    [AddComponentMenu("OmiLAXR / Modules / ReCoPa")]
    public class ReCoPa : Module, IDebugSender
    {
        public string connectionUrl = "http://127.0.0.1:4567";

        // variables for websocket communication
        private SocketIOUnity _socket;

        private LearnerPipeline _learnerPipeline;
        private LearningRecordStore _learningRecordStore;

        private Coroutine _scenarioUpdateCoroutine;
        private bool _wasTracking;

        public bool IsConnected => _socket != null && _socket.Connected;
        public UnityEvent onConnected = new UnityEvent();
        public UnityEvent onDisconnected = new UnityEvent();
        public UnityEvent onReconnected = new UnityEvent();

        private void Start()
        {
            _learnerPipeline = FindObjectOfType<LearnerPipeline>();
            _learningRecordStore = FindObjectOfType<LearningRecordStore>();

            Init();
            InitSocket();

            SceneManager.sceneLoaded += ChangedScene;
        }

        private void ChangedScene(Scene arg0, LoadSceneMode arg1)
        {
            // Init();
            // if (_wasTracking)
            // {
            //     //trackingSystem.StartTracking();
            // }
        }

        private void Init()
        {
            // if (eyeTrackingSystem)
            // {
            //     eyeTrackingSystem.OnCalibrationStarted += _ => SendMeta("calibration:start");
            //     eyeTrackingSystem.OnCalibrationStopped += _ => SendMeta("calibration:stop");
            // }
            // else
            // {
            //     DebugLog.Warning("No EyeTrackingSystem is set.");
            // }

            // Disable learner tracking

            if (_learnerPipeline)
            {
                DebugLog.Warning("Cannot find a <LearnerPipeline>.");
                return;
            }
            
            _learnerPipeline.gameObject.SetActive(false);

            _learnerPipeline.onStartedPipeline += () =>
            {
                _wasTracking = true;
                SendMeta("tracking:start");
            };
            _learnerPipeline.onStoppedPipeline += () =>
            {
                _wasTracking = false;
                SendMeta("tracking:stop");
            };
            _learnerPipeline.AfterFilteredObjects += (objects) =>
            {
                var config = new TrackingConfig();
                config.gameObjects = objects.Select(o => o.name).ToArray();
                
                _socket.Emit("clients:tracking", JObject.FromObject(config));
            };
            
            // trackingSystem.MakeDirty();
        }
        
        private void InitSocket()
        {
            // stop if a socket is assigned
            if (_socket != null)
                return;

            _socket = new SocketIOUnity(connectionUrl, new SocketIOOptions()
            {
                Transport = TransportProtocol.WebSocket,
                Reconnection = true,
                ReconnectionDelay = 5000,
                ReconnectionDelayMax = 10000,
                ReconnectionAttempts = 1000,
                ExtraHeaders = new Dictionary<string, string>()
                {
                    ["clientType"] = "participant"
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

            // _socket.On("clients:all", _ => trackingSystem.MakeDirty());

            _socket.On("clients:scenario", DispatchScenarioInformation);

            // _socket.On("clients:calibration:start", _ => eyeTrackingSystem.StartCalibration());
            // _socket.On("clients:calibration:stop", _ => eyeTrackingSystem.StopCalibration());

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
            DebugLog.Print("[ReCoPa Connector] Connected to ReCoPa.");
            onConnected.Invoke();
            SendMeta("connected");
            BeginScenarioUpdate();
        }

        private void OnReconnected()
        {
            DebugLog.Print("[ReCoPa Connector] Reconnected to ReCoPa.");
            onReconnected.Invoke();
            SendMeta("reconnected");
            BeginScenarioUpdate();
        }

        private void BeginScenarioUpdate()
        {
            // trackingSystem.MakeDirty();
            // UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            // {
            //     _scenarioUpdateCoroutine = StartCoroutine(UpdateScenario());
            // });
        }

        private void OnDisconnected()
        {
            DebugLog.Print("[ReCoPa Connector] Disconnected from ReCoPa.");
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
                // _socket.EmitAsync("clients:meta", trackingSystem.GetMeta(metaContext));
            });
        }
        //
        // private IEnumerator UpdateScenario()
        // {
        //     while (true)
        //     {
        //         if (_socket.Connected && trackingSystem.IsDirty())
        //         {
        //             SendScenario();
        //         }
        //
        //         yield return new WaitForSeconds(5);
        //     }
        // }

        /// <summary>
        /// Sends all gameObjects and actions to socket server.
        /// </summary>
        private void SendScenario(bool reload = false)
        {
            if (_socket == null)
                return;
            
            // var scenario = trackingSystem.GetScenario(reload);
            // var tracking = trackingSystem.GetTrackingConfig(scenario);
            //
            // // transfer resulted JSONObject to socket server
            // _socket.EmitAsync("clients:scenario", scenario);
            // _socket.EmitAsync("clients:tracking", tracking);
            //
            // DebugLog.Print("Sent scenario information.");
            //
            // trackingSystem.TidyUp();
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
                // trackingSystem.StartTracking(config);
            });
        }
        
        private void DispatchPauseTracking(SocketIOResponse e)
        {
            throw new NotImplementedException("Todo");
        }
        
        private void DispatchResumeTracking(SocketIOResponse e)
        {
            throw new NotImplementedException("Todo");
        }

        private void DispatchStopTracking(SocketIOResponse e)
        {
            // UnityMainThreadDispatcher.Instance().EnqueueAsync(() => { trackingSystem.StopTracking(); });
        }

        private void DispatchTrackingInformation(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                // var tracking = trackingSystem.GetScenarioTrackingConfig();
                // _socket.EmitAsync("clients:tracking", JObject.FromObject(tracking));
            });
        }

        private void DispatchScenarioInformation(SocketIOResponse e)
        {
            UnityMainThreadDispatcher.Instance().EnqueueAsync(() =>
            {
                // var scenario = trackingSystem.GetScenario();
                // _socket.EmitAsync("clients:scenario", JObject.FromObject(scenario));
            });
        }

        private static readonly DebugLog Debug = new DebugLog("ReCoPa Connector");
        public DebugLog DebugLog => Debug;
    }

}