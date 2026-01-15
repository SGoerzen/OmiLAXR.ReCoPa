using System;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace OmiLAXR.ReCoPa.Network
{
    // ------------------------------------------------------------
    // SocketIOResponse-like wrapper (GetValue<T>())
    // ------------------------------------------------------------
    public sealed class SocketResponse
    {
        private readonly string _raw;
        private readonly IJsonSerializer _serializer;

        public SocketResponse(string raw, IJsonSerializer serializer)
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
}