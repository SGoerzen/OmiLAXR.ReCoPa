using System.Collections.Generic;

namespace OmiLAXR.ReCoPa.Network
{
    public sealed class SocketClientOptions
    {
        // Timeouts (ms)
        public int ConnectTimeoutMs = 5000;
        public int SendTimeoutMs = 5000;
        public int ReceiveTimeoutMs = 30000;

        // Reconnect (SocketIOUnity-like)
        public bool Reconnection = true;
        public int ReconnectionDelay = 30_000;
        public int ReconnectionDelayMax = 60_000;
        public int ReconnectionAttempts = 10; // <=0 => unlimited

        // TCP
        public bool NoDelay = true;
        public bool KeepAlive = true;

        // Payload sizing
        public int MaxMessageBytes = 1024 * 1024;

        // Mild exponential backoff on top of ReconnectionDelay (optional)
        public double ReconnectBackoffFactor = 1.4;

        // For non-Unity: post events back to captured SynchronizationContext
        public bool UseSynchronizationContext = true;

        // Socket.IO "ExtraHeaders" (TCP has no headers -> sent once as "clients:hello")
        public Dictionary<string, string> ExtraHeaders = new Dictionary<string, string>();
    }
}