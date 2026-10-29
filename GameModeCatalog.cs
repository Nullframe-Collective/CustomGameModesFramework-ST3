namespace CustomGameModesFramework
{
    public static class GameModeCatalog
    {
        private static bool _enabled = true;

        public static int ApiVersion => 1;
        public static bool IsReady => Core.IsInitialized;

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;

                _enabled = value;
                GameModeMenu.Invalidate();
            }
        }
    }
}
