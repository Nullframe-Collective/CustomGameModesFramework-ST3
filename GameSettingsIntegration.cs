using Il2Cpp;
using MelonLoader;
using System.Reflection;
using UnityEngine;

namespace CustomGameModesFramework
{
    internal static class GameSettingsIntegration
    {
        private static IDisposable _registration;
        private static Func<LobbyMenu, bool> _isActive;
        private static volatile bool _checked;

        internal static bool LegacyNullServers { get; private set; }

        internal static void AssemblyLoaded() => _checked = false;

        internal static void Initialize()
        {
            if (_registration != null || _checked) return;

            _checked = true;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type api = assembly.GetType("NullServers.UI.GameSettingsExtensions");

                if (api == null)
                {
                    if (assembly.GetType("NullServers.Room.RoomCustomization") != null)
                    {
                        if (LegacyNullServers) continue;

                        LegacyNullServers = true;
                        MelonLogger.Warning("Inline mode settings require NullServers 0.0.26 or newer. Update both DLLs from the release archive.");
                    }

                    continue;
                }

                Func<LobbyMenu, float, float, float, float> measure = GameModeMenu.SettingsHeight;
                Action<LobbyMenu, Rect, float, float> draw = GameModeMenu.DrawSettings;
                _registration = (IDisposable)api.GetMethod("Register").Invoke(null, new object[]
                {
                    "CustomGameModesFramework", measure, draw
                });
                _isActive = api.GetMethod("IsActive").CreateDelegate<Func<LobbyMenu, bool>>();
                LegacyNullServers = false;

                return;
            }
        }

        internal static bool IsActive(LobbyMenu menu) => _isActive?.Invoke(menu) == true;

        internal static void Dispose()
        {
            _registration?.Dispose();
            _registration = null;
            _isActive = null;
            _checked = false;
            LegacyNullServers = false;
        }
    }
}
