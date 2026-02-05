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
        public int ReconnectionDelay = 5_000;              // Start with 5s
        public int ReconnectionDelayMax = 30_000;          // Max 30s between attempts
        public int ReconnectionAttempts = -1;              // -1 = unlimited, attempts forever
        public double ReconnectBackoffFactor = 1.5;        // Exponential backoff factor

        // Heartbeat / Ping-Pong für Stabilitaet
        public bool EnableHeartbeat = true;
        public int HeartbeatIntervalMs = 15_000;           // Ping alle 15 Sekunden
        public int HeartbeatTimeoutMs = 5_000;             // Pong timeout nach 5 Sekunden

        // TCP
        public bool NoDelay = true;
        public bool KeepAlive = true;                     // Enables OS-level TCP keep-alive
        public int TcpKeepAliveIdleSeconds = 30;           // Seconds before first keep-alive probe
        public int TcpKeepAliveIntervalSeconds = 5;        // Interval between probes
        public int TcpKeepAliveProbes = 3;                 // Number of probes before giving up

        // Payload sizing
        public int MaxMessageBytes = 1024 * 1024;

        // For non-Unity: post events back to captured SynchronizationContext
        public bool UseSynchronizationContext = true;

        // Socket.IO "ExtraHeaders" (TCP has no headers -> sent once as "clients:hello")
        public Dictionary<string, string> ExtraHeaders = new Dictionary<string, string>();
    }
}