/*
* SPDX-License-Identifier: AGPL-3.0-or-later
* Copyright (C) 2025 Sergej Görzen <sergej.goerzen@gmail.com>
* This file is part of OmiLAXR.
*/
using System.Collections.Generic;

namespace OmiLAXR.ReCoPa.Network
{
    /// <summary>
    /// Configuration options for <see cref="SocketClient"/>.
    /// Controls timeouts, reconnection policy, heartbeat, and buffering behavior.
    /// </summary>
    public sealed class SocketClientOptions
    {
        // Timeouts (ms)
        /// <summary>
        /// Connection timeout in milliseconds.
        /// </summary>
        public int ConnectTimeoutMs = 5000;

        /// <summary>
        /// Send timeout in milliseconds.
        /// </summary>
        public int SendTimeoutMs = 5000;

        /// <summary>
        /// Receive timeout in milliseconds.
        /// </summary>
        public int ReceiveTimeoutMs = 30000;

        // Reconnect (SocketIOUnity-like)
        /// <summary>
        /// Enables automatic reconnection.
        /// </summary>
        public bool Reconnection = true;

        /// <summary>
        /// Delay before the first reconnection attempt in milliseconds.
        /// </summary>
        public int ReconnectionDelay = 5_000;              // Start with 5s

        /// <summary>
        /// Maximum delay between reconnection attempts in milliseconds.
        /// </summary>
        public int ReconnectionDelayMax = 30_000;          // Max 30s between attempts

        /// <summary>
        /// Number of reconnection attempts; -1 means unlimited.
        /// </summary>
        public int ReconnectionAttempts = -1;              // -1 = unlimited, attempts forever

        /// <summary>
        /// Backoff factor applied to reconnection delays.
        /// </summary>
        public double ReconnectBackoffFactor = 1.5;        // Exponential backoff factor

        // Heartbeat / Ping-Pong für Stabilitaet
        /// <summary>
        /// Enables heartbeat ping/pong to detect stale connections.
        /// </summary>
        public bool EnableHeartbeat = true;

        /// <summary>
        /// Heartbeat ping interval in milliseconds.
        /// </summary>
        public int HeartbeatIntervalMs = 15_000;           // Ping alle 15 Sekunden

        /// <summary>
        /// Heartbeat timeout in milliseconds.
        /// </summary>
        public int HeartbeatTimeoutMs = 5_000;             // Pong timeout nach 5 Sekunden

        // TCP
        /// <summary>
        /// Disables Nagle's algorithm for lower latency.
        /// </summary>
        public bool NoDelay = true;

        /// <summary>
        /// Enables OS-level TCP keep-alive probes.
        /// </summary>
        public bool KeepAlive = true;                     // Enables OS-level TCP keep-alive

        /// <summary>
        /// Idle time before the first TCP keep-alive probe (seconds).
        /// </summary>
        public int TcpKeepAliveIdleSeconds = 30;           // Seconds before first keep-alive probe

        /// <summary>
        /// Interval between TCP keep-alive probes (seconds).
        /// </summary>
        public int TcpKeepAliveIntervalSeconds = 5;        // Interval between probes

        /// <summary>
        /// Number of TCP keep-alive probes before giving up.
        /// </summary>
        public int TcpKeepAliveProbes = 3;                 // Number of probes before giving up

        // Payload sizing
        /// <summary>
        /// Maximum payload size allowed per message.
        /// </summary>
        public int MaxMessageBytes = 1024 * 1024;

        // For non-Unity: post events back to captured SynchronizationContext
        /// <summary>
        /// When true, events are dispatched via the captured <see cref="System.Threading.SynchronizationContext"/>.
        /// </summary>
        public bool UseSynchronizationContext = true;

        // Socket.IO "ExtraHeaders" (TCP has no headers -> sent once as "clients:hello")
        /// <summary>
        /// Extra headers to send once on connect as a "clients:hello" payload.
        /// </summary>
        public Dictionary<string, string> ExtraHeaders = new Dictionary<string, string>();

        // Outgoing buffering when disconnected
        /// <summary>
        /// Buffer outgoing messages while disconnected instead of throwing.
        /// </summary>
        public bool BufferOutgoingWhenDisconnected = true; // if true, Emit will enqueue instead of throwing

        /// <summary>
        /// Maximum number of buffered messages before dropping the oldest.
        /// </summary>
        public int MaxBufferedMessages = 1000; // cap for buffered messages
    }
}
