namespace CustomGameModesFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GameModeInfo : Attribute
    {
        public string ModeID;
        public string ModeName;
        public string Players;
        public string SmallMapPlayerCount;
        public string MediumMapPlayerCount;
        public string LargeMapPlayerCount;

        public string BaseMode { get; set; } = "SBX";

        internal static readonly HashSet<string> NativeModes = new(StringComparer.Ordinal)
        {
            "COOP", "VS", "SUR", "INF", "SBX", "THR", "DM", "TDM"
        };

        public GameModeInfo(string modeID, string modeName, string players, string smallMapPlayerCount,
            string mediumMapPlayerCount, string largeMapPlayerCount)
        {
            ModeID = modeID;
            ModeName = modeName;
            Players = players;
            SmallMapPlayerCount = smallMapPlayerCount;
            MediumMapPlayerCount = mediumMapPlayerCount;
            LargeMapPlayerCount = largeMapPlayerCount;
        }
    }
}
