using System;
using System.Net.Sockets;

namespace OmiLAXR.ReCoPa.Network
{
    public class UnitySocketClient : SocketClient
    {
        public enum UnityThreadScope
        {
            Update,
            FixedUpdate,
            LateUpdate
        }
        
        private readonly UnityThreadScope _scope;

        // Overload wie bei SocketIOUnity(..., UnityThreadScope.FixedUpdate)
        public UnitySocketClient(string connectionUrl, SocketClientOptions options, UnityThreadScope scope)
            : base(connectionUrl, options)
        {
            _scope = scope; // aktuell nur "informational"
        }

        // Like SocketIOUnity.OnUnityThread("event", cb)
        public void OnUnityThread(string eventName, Action<SocketIOResponse> callback)
        {
            // Wir registrieren normal, dispatchen aber auf Unity main thread
            On(eventName, resp =>
            {
                // nutzt dein vorhandenes UnityMainThreadDispatcher
                UnityMainThreadDispatcher.Instance().EnqueueAsync(() => callback(resp));
            });
        }
    }
}