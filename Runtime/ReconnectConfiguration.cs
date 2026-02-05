namespace OmiLAXR.ReCoPa.Network
{
    /// <summary>
    /// Vordefinierte Reconnect-Konfigurationen für verschiedene Szenarien
    /// </summary>
    public static class ReconnectConfiguration
    {
        /// <summary>
        /// Aggressive Reconnection - schnelle Wiederverbindung (für LAN/Lokal-Netzwerke)
        /// </summary>
        public static SocketClientOptions CreateAggressiveConfig()
        {
            return new SocketClientOptions
            {
                Reconnection = true,
                ReconnectionDelay = 2_000,              // Start nach 2 Sekunden
                ReconnectionDelayMax = 10_000,          // Max 10 Sekunden
                ReconnectionAttempts = -1,              // Unbegrenzte Versuche
                ReconnectBackoffFactor = 1.5,
                
                EnableHeartbeat = true,
                HeartbeatIntervalMs = 5_000,            // Ping alle 5 Sekunden
                HeartbeatTimeoutMs = 3_000,             // Timeout nach 3 Sekunden
                
                ConnectTimeoutMs = 3_000,
                SendTimeoutMs = 3_000,
                ReceiveTimeoutMs = 10_000,
            };
        }

        /// <summary>
        /// Moderates Reconnect - ausgewogenes Setup (Standard)
        /// </summary>
        public static SocketClientOptions CreateModerateConfig()
        {
            return new SocketClientOptions
            {
                Reconnection = true,
                ReconnectionDelay = 5_000,              // Start nach 5 Sekunden
                ReconnectionDelayMax = 30_000,          // Max 30 Sekunden
                ReconnectionAttempts = -1,              // Unbegrenzte Versuche
                ReconnectBackoffFactor = 1.5,
                
                EnableHeartbeat = true,
                HeartbeatIntervalMs = 15_000,           // Ping alle 15 Sekunden
                HeartbeatTimeoutMs = 5_000,             // Timeout nach 5 Sekunden
                
                ConnectTimeoutMs = 5_000,
                SendTimeoutMs = 5_000,
                ReceiveTimeoutMs = 30_000,
            };
        }

        /// <summary>
        /// Konservatives Reconnect - robuster für unstabile Verbindungen
        /// </summary>
        public static SocketClientOptions CreateConservativeConfig()
        {
            return new SocketClientOptions
            {
                Reconnection = true,
                ReconnectionDelay = 10_000,             // Start nach 10 Sekunden
                ReconnectionDelayMax = 60_000,          // Max 60 Sekunden
                ReconnectionAttempts = -1,              // Unbegrenzte Versuche
                ReconnectBackoffFactor = 2.0,           // Steilerer Backoff
                
                EnableHeartbeat = true,
                HeartbeatIntervalMs = 30_000,           // Ping alle 30 Sekunden
                HeartbeatTimeoutMs = 10_000,            // Timeout nach 10 Sekunden
                
                ConnectTimeoutMs = 10_000,
                SendTimeoutMs = 10_000,
                ReceiveTimeoutMs = 60_000,
            };
        }

        /// <summary>
        /// Beispiel: Verwendung in ReCoPa
        /// </summary>
        public static void Example()
        {
            // Auswahl der richtigen Konfiguration
            var options = CreateModerateConfig(); // oder Aggressive/Conservative

            // Mit ReCoPa verwenden:
            // var reCoPa = new ReCoPa("ws://localhost:4567", options);
            // reCoPa.Connect().ConfigureAwait(false);
        }
    }
}
