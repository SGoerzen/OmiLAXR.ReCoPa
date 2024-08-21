namespace OmiLAXR.Modules.ReCoPa
{
    public struct TrackingScenario
    {
        public string[] gameObjects;
        public string[] actions;
        public string[] gestures;
        public string name;
        
        public override string ToString()
        {
            return $"[TrackingScenario name={name}, actions={Array(actions)}, gestures={Array(gestures)}, gameObjects={Array(gameObjects)}]";
        }
        
        private static string Array(string[] array)
        {
            var str = array != null ? string.Join(",", array) : null;
            return $"[Array: [{str}]]";
        }
    }

}