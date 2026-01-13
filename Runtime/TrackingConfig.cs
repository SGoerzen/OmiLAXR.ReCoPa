using OmiLAXR.Utils;

namespace OmiLAXR.ReCoPa
{
    public struct TrackingConfig
    {
        public struct ClientAuth
        {
            public string key;
            public string secret;
            public ClientAuth(string key, string secret)
            {
                this.key = key;
                this.secret = secret;
            }

            public override string ToString()
            {
                return $"[ClientAuth: key={key}, secret={secret}]";
            }
        }
        
        public struct TrackingIdentity
        {
            public string email;
            public string name;
        
            public TrackingIdentity(string name, string email)
            {
                this.name = name;
                this.email = email;
            }
        
            public override string ToString()
            {
                return $"[TrackingIdentity email={email}, name={name}]";
            }
        }

        
        public string lrs;
        public string uri;
        public ClientAuth auth;
        public TrackingIdentity identity;
        public string[] gameObjects;
        public string[] actions;
        public string[] gestures;
        public bool isBlacklist;
        public EndpointConfigs endpoints;

        public override string ToString()
        {
            return $"[TrackingConfig: lrs={lrs}, uri={uri}, auth={auth}, identity={identity}, gameObjects={Array(gameObjects)}, actions={Array(actions)}, gestures={Array(gestures)}, isBlackList={isBlacklist}]";
        }

        private static string Array(string[] array)
        {
            var str = array != null ? string.Join(",", array) : null;
            return $"[Array: [{str}]]";
        }
    }
}

