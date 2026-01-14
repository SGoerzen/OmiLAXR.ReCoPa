#nullable enable
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OmiLAXR.ReCoPa.Network
{
    // ------------------------------------------------------------
    // JSON Serializer abstraction (default: Newtonsoft)
    // ------------------------------------------------------------
    public interface IJsonSerializer
    {
        string Serialize(object? value);
        T Deserialize<T>(string json);
    }

    public sealed class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public string Serialize(object? value)
        {
            if (value == null) return string.Empty;
            if (value is string s) return s;
            if (value is JToken jt) return jt.ToString(Formatting.None);
            return JsonConvert.SerializeObject(value);
        }

        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default!;
            return JsonConvert.DeserializeObject<T>(json)!;
        }
    }

    // ------------------------------------------------------------
    // SocketIOResponse-like wrapper (GetValue<T>())
    // ------------------------------------------------------------
    public sealed class SocketIOResponse
    {
        private readonly string _raw;
        private readonly IJsonSerializer _serializer;

        public SocketIOResponse(string raw, IJsonSerializer serializer)
        {
            _raw = raw ?? string.Empty;
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public string RawText => _raw;
        public T GetValue<T>() => _serializer.Deserialize<T>(_raw);

        public JToken GetToken()
        {
            if (string.IsNullOrWhiteSpace(_raw)) return JValue.CreateNull();
            return JToken.Parse(_raw);
        }
    }

    // ------------------------------------------------------------
    // Options (SocketIOUnity-like fields)
    // ------------------------------------------------------------
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

    // ------------------------------------------------------------
    // CLIENT: SocketClient (pure C#)
    // ------------------------------------------------------------
    public partial class SocketClient : IDisposable
    {
        private readonly string _connectionUrl;
        private readonly SocketClientOptions _opt;

        private IJsonSerializer _serializer = new NewtonsoftJsonSerializer();

        private readonly object _gate = new();
        private readonly Dictionary<string, List<Action<SocketIOResponse>>> _handlers = new(StringComparer.Ordinal);

        private TcpClient? _tcp;
        private NetworkStream? _stream;

        private CancellationTokenSource? _cts;
        private Task? _runTask;

        private bool _disposed;
        private bool _everConnected;
        private int _attempt;

        private SynchronizationContext? _syncContext;

        // SocketIOUnity-like events
        public event EventHandler? OnConnected;
        public event EventHandler? OnReconnected;
        public event EventHandler? OnDisconnected;

        public event EventHandler<int>? OnReconnectAttempt;
        public event EventHandler<Exception>? OnReconnectError;
        public event EventHandler? OnReconnectFailed;

        public event EventHandler<string>? OnError;

        public bool Connected => _tcp?.Connected == true;

        public IJsonSerializer JsonSerializer
        {
            get => _serializer;
            set => _serializer = value ?? throw new ArgumentNullException(nameof(value));
        }

        public SocketClient(string connectionUrl, SocketClientOptions options)
        {
            _connectionUrl = connectionUrl ?? throw new ArgumentNullException(nameof(connectionUrl));
            _opt = options ?? throw new ArgumentNullException(nameof(options));

            _syncContext = (_opt.UseSynchronizationContext ? SynchronizationContext.Current : null);
        }

        // Like SocketIOUnity.On("event", cb)
        public void On(string eventName, Action<SocketIOResponse> callback)
        {
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            lock (_gate)
            {
                if (!_handlers.TryGetValue(eventName, out var list))
                {
                    list = new List<Action<SocketIOResponse>>();
                    _handlers[eventName] = list;
                }
                list.Add(callback);
            }
        }

        // Like SocketIOUnity.Emit / EmitAsync
        public void Emit(string eventName, object data) => _ = EmitAsync(eventName, data);

        public Task EmitAsync(string eventName, object data)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SocketClient));
            if (!Connected || _stream == null) throw new InvalidOperationException("SocketClient is not connected.");

            string payload = _serializer.Serialize(data);

            return Framing.WriteMessageAsync(
                _stream,
                eventName,
                payload,
                _opt.MaxMessageBytes,
                TimeSpan.FromMilliseconds(Math.Max(0, _opt.SendTimeoutMs)),
                _cts?.Token ?? CancellationToken.None
            );
        }

        // Like SocketIOUnity.ConnectAsync()
        public Task ConnectAsync()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SocketClient));
            if (_runTask != null) return Task.CompletedTask;

            _cts = new CancellationTokenSource();

            if (_opt.UseSynchronizationContext && _syncContext == null)
                _syncContext = SynchronizationContext.Current;

            _runTask = Task.Run(() => RunLoopAsync(_cts.Token));
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
            if (_disposed) return;
            try { _cts?.Cancel(); } catch { }
            SafeClose();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Disconnect();
            try { _cts?.Dispose(); } catch { }
            _cts = null;
            _runTask = null;
        }

        // ------------------------------------------------------------
        // Internals
        // ------------------------------------------------------------
        private async Task RunLoopAsync(CancellationToken ct)
        {
            var (host, port) = ParseHostPort(_connectionUrl);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await ConnectOnceAsync(host, port, ct).ConfigureAwait(false);

                    // TCP has no headers -> send once as hello event
                    if (_opt.ExtraHeaders != null && _opt.ExtraHeaders.Count > 0)
                    {
                        var helloObj = new JObject
                        {
                            ["headers"] = JObject.FromObject(_opt.ExtraHeaders),
                            ["ts"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                        };
                        await EmitAsync("clients:hello", helloObj).ConfigureAwait(false);
                    }

                    if (_everConnected) RaiseOnContext(() => OnReconnected?.Invoke(this, EventArgs.Empty));
                    else RaiseOnContext(() => OnConnected?.Invoke(this, EventArgs.Empty));

                    _everConnected = true;
                    _attempt = 0;

                    await ReceiveLoopAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    RaiseOnContext(() => OnError?.Invoke(this, ex.Message));
                    RaiseOnContext(() => OnReconnectError?.Invoke(this, ex));
                }
                finally
                {
                    SafeClose();
                    RaiseOnContext(() => OnDisconnected?.Invoke(this, EventArgs.Empty));
                }

                if (ct.IsCancellationRequested) break;
                if (!_opt.Reconnection) break;

                _attempt++;
                RaiseOnContext(() => OnReconnectAttempt?.Invoke(this, _attempt));

                if (_opt.ReconnectionAttempts > 0 && _attempt > _opt.ReconnectionAttempts)
                {
                    RaiseOnContext(() => OnReconnectFailed?.Invoke(this, EventArgs.Empty));
                    break;
                }

                var delay = ComputeReconnectDelay(_attempt);
                try { await Task.Delay(delay, ct).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task ConnectOnceAsync(string host, int port, CancellationToken ct)
        {
            SafeClose();

            var tcp = new TcpClient();
            ConfigureSocket(tcp);

            var connectTask = tcp.ConnectAsync(host, port);

            int timeout = Math.Max(0, _opt.ConnectTimeoutMs);
            if (timeout > 0)
            {
                var timeoutTask = Task.Delay(timeout, ct);
                var done = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);
                if (done == timeoutTask)
                    throw new TimeoutException($"Connect timeout after {timeout}ms.");
            }

            await connectTask.ConfigureAwait(false);

            _tcp = tcp;
            _stream = tcp.GetStream();
        }

        private async Task ReceiveLoopAsync(CancellationToken ct)
        {
            if (_stream == null) throw new InvalidOperationException("No stream.");

            while (!ct.IsCancellationRequested)
            {
                var (ev, payload) = await Framing.ReadMessageAsync(
                    _stream,
                    _opt.MaxMessageBytes,
                    TimeSpan.FromMilliseconds(Math.Max(0, _opt.ReceiveTimeoutMs)),
                    ct
                ).ConfigureAwait(false);

                Dispatch(ev, payload);
            }
        }

        private void Dispatch(string eventName, string payload)
        {
            List<Action<SocketIOResponse>>? list;

            lock (_gate)
            {
                _handlers.TryGetValue(eventName, out list);
                list = list == null ? null : new List<Action<SocketIOResponse>>(list);
            }

            if (list == null) return;

            var resp = new SocketIOResponse(payload, _serializer);

            foreach (var cb in list)
            {
                try { cb(resp); }
                catch (Exception ex) { RaiseOnContext(() => OnError?.Invoke(this, ex.Message)); }
            }
        }

        protected void RaiseOnContext(Action a)
        {
            if (!_opt.UseSynchronizationContext || _syncContext == null)
            {
                a();
                return;
            }
            _syncContext.Post(_ => a(), null);
        }

        private TimeSpan ComputeReconnectDelay(int attempt)
        {
            double factor = Math.Max(1.0, _opt.ReconnectBackoffFactor);
            double baseMs = Math.Max(0, _opt.ReconnectionDelay);
            double maxMs = Math.Max(baseMs, _opt.ReconnectionDelayMax);

            double ms = baseMs * Math.Pow(factor, Math.Max(0, attempt - 1));
            ms = Math.Min(ms, maxMs);

            // jitter +/- 10%
            double jitter = ms * 0.1;
            double r = ThreadSafeRandom.NextDouble();
            ms = ms + (r * 2.0 - 1.0) * jitter;

            return TimeSpan.FromMilliseconds(Math.Max(0, ms));
        }

        private void ConfigureSocket(TcpClient tcp)
        {
            try
            {
                tcp.NoDelay = _opt.NoDelay;
                if (_opt.KeepAlive) tcp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
            catch { /* ignore platform limits */ }
        }

        private void SafeClose()
        {
            try { _stream?.Close(); } catch { }
            try { _tcp?.Close(); } catch { }
            _stream = null;
            _tcp = null;
        }

        private static (string host, int port) ParseHostPort(string urlOrHostPort)
        {
            const int defaultPort = 4567;

            if (string.IsNullOrWhiteSpace(urlOrHostPort))
                return ("127.0.0.1", defaultPort);

            if (Uri.TryCreate(urlOrHostPort, UriKind.Absolute, out var uri))
            {
                var host = uri.Host;
                var port = uri.IsDefaultPort ? defaultPort : uri.Port;
                return (host, port);
            }

            var parts = urlOrHostPort.Split(':');
            if (parts.Length == 2 && int.TryParse(parts[1], out var p))
                return (parts[0], p);

            return (urlOrHostPort, defaultPort);
        }

        // ------------------------------------------------------------
        // Framing: [int32 bodyLen BE][uint16 eventLen BE][event UTF8][payload UTF8]
        // ------------------------------------------------------------
        private static class Framing
        {
            public static async Task WriteMessageAsync(
                NetworkStream stream,
                string eventName,
                string payload,
                int maxMessageBytes,
                TimeSpan sendTimeout,
                CancellationToken ct)
            {
                if (stream == null) throw new ArgumentNullException(nameof(stream));
                if (eventName == null) throw new ArgumentNullException(nameof(eventName));
                payload ??= string.Empty;

                var evBytes = Encoding.UTF8.GetBytes(eventName);
                if (evBytes.Length > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(eventName), "eventName too long.");

                var plBytes = Encoding.UTF8.GetBytes(payload);

                int bodyLen = 2 + evBytes.Length + plBytes.Length;
                if (bodyLen <= 0 || bodyLen > maxMessageBytes)
                    throw new InvalidDataException($"Message too large: {bodyLen} > {maxMessageBytes}");

                byte[] frame = new byte[4 + bodyLen];
                BinaryPrimitives.WriteInt32BigEndian(frame.AsSpan(0, 4), bodyLen);
                BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(4, 2), (ushort)evBytes.Length);

                Buffer.BlockCopy(evBytes, 0, frame, 6, evBytes.Length);
                Buffer.BlockCopy(plBytes, 0, frame, 6 + evBytes.Length, plBytes.Length);

                using var linked = CreateTimeoutCts(sendTimeout, ct);
                await stream.WriteAsync(frame, 0, frame.Length, linked.Token).ConfigureAwait(false);
                await stream.FlushAsync(linked.Token).ConfigureAwait(false);
            }

            public static async Task<(string EventName, string Payload)> ReadMessageAsync(
                NetworkStream stream,
                int maxMessageBytes,
                TimeSpan receiveTimeout,
                CancellationToken ct)
            {
                byte[] lenBuf = new byte[4];
                await ReadExactAsync(stream, lenBuf, receiveTimeout, ct).ConfigureAwait(false);

                int bodyLen = BinaryPrimitives.ReadInt32BigEndian(lenBuf.AsSpan());
                if (bodyLen <= 0 || bodyLen > maxMessageBytes)
                    throw new InvalidDataException($"Invalid body length {bodyLen} (limit {maxMessageBytes}).");

                byte[] body = new byte[bodyLen];
                await ReadExactAsync(stream, body, receiveTimeout, ct).ConfigureAwait(false);

                ushort evLen = BinaryPrimitives.ReadUInt16BigEndian(body.AsSpan(0, 2));
                if (evLen == 0 || 2 + evLen > bodyLen)
                    throw new InvalidDataException("Invalid event name length.");

                string ev = Encoding.UTF8.GetString(body, 2, evLen);
                string payload = Encoding.UTF8.GetString(body, 2 + evLen, bodyLen - (2 + evLen));
                return (ev, payload);
            }

            private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, TimeSpan timeout, CancellationToken ct)
            {
                int offset = 0;
                using var linked = CreateTimeoutCts(timeout, ct);

                while (offset < buffer.Length)
                {
                    int read = await stream.ReadAsync(buffer, offset, buffer.Length - offset, linked.Token).ConfigureAwait(false);
                    if (read == 0) throw new EndOfStreamException("Remote closed connection.");
                    offset += read;
                }
            }

            private static CancellationTokenSource CreateTimeoutCts(TimeSpan timeout, CancellationToken ct)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                if (timeout > TimeSpan.Zero) cts.CancelAfter(timeout);
                return cts;
            }
        }

        private static class ThreadSafeRandom
        {
            private static readonly object _lock = new();
            private static readonly Random _rnd = new();

            public static double NextDouble()
            {
                lock (_lock) return _rnd.NextDouble();
            }
        }
    }
}
