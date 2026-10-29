using HarmonyLib;
using Il2Cpp;
using System.Reflection;
using UnityEngine;

namespace CustomGameModesFramework
{
    internal static class GameSettingsPanel
    {
        private static LobbyMenu _menu;
        private static Rect _panel;
        private static Rect _viewport;
        private static Vector2 _scroll;
        private static float _width;
        private static float _bottom;
        private static float _sectionY;
        private static float _sectionHeight;
        private static bool _modeScrollOpen;
        private static IntPtr _lastMenu;
        private static string _lastMode;

        internal static void Begin(LobbyMenu menu)
        {
            _menu = null;
            _panel = default;

            GameSettingsIntegration.Initialize();

            if (GameSettingsIntegration.IsActive(menu) || GameSettingsIntegration.LegacyNullServers) return;

            GameModeRegistration registration = GameModeMenu.Selected(menu);

            if (registration == null || registration.Settings.Entries.Count == 0 || menu.EJLDOIOJGPC) return;

            if (_lastMenu != menu.Pointer || _lastMode != registration.Info.ModeID)
            {
                _lastMenu = menu.Pointer;
                _lastMode = registration.Info.ModeID;
                _scroll = Vector2.zero;
            }

            _menu = menu;
        }

        internal static void Capture(Rect rect)
        {
            if (!_menu || rect.x < Screen.width * 0.5f || rect.height < Screen.height * 0.5f) return;

            _panel = rect;
            float padding = Mathf.Max(6f, Screen.width * 0.006f);
            float header = Mathf.Max(24f, Screen.height * 0.05f);
            _viewport = new Rect(rect.x + padding, rect.y + header, rect.width - padding * 2f, rect.height - header - padding);
            _width = _viewport.width - Mathf.Max(16f, GUI.skin.verticalScrollbar.fixedWidth) - 4f;
            _bottom = _viewport.y;
            _sectionY = -1f;
            float row = Mathf.Max(13, Screen.width / 70) * 1.8f;
            _sectionHeight = GameModeMenu.SettingsHeight(_menu, _width, row, row / 1.8f * 0.7f);
        }

        internal static void Clip(ref Rect rect, out bool active, bool footer)
        {
            active = _menu && !_modeScrollOpen && _panel.width > 0f &&
                rect.x >= _panel.x - 1f && rect.xMax <= _panel.xMax + 1f &&
                rect.y >= _viewport.y && rect.yMax <= _panel.yMax + 1f;

            if (!active) return;

            if (!_menu.EGDMPMDBJIK)
            {
                if (footer)
                {
                    _sectionY = _bottom + Mathf.Max(8f, Screen.width * 0.01f);
                    rect.y = _sectionY + _sectionHeight;
                }

                _bottom = Mathf.Max(_bottom, rect.yMax);
            }

            Rect clip = _viewport;
            clip.width = _width;
            GUI.BeginGroup(clip);
            rect.position -= _viewport.position + _scroll;
        }

        internal static void EndClip(bool active)
        {
            if (active)
            {
                GUI.EndGroup();
            }
        }

        internal static void BeginModeScroll(ref Rect rect, ref Rect content)
        {
            if (!_menu || !_menu.EGDMPMDBJIK || _panel.width <= 0f || rect.x < _panel.x || rect.x >= _panel.xMax) return;

            Rect clip = _viewport;
            clip.width = _width;
            GUI.BeginGroup(clip);
            rect.position -= clip.position + _scroll;
            rect.width = Mathf.Min(rect.width, clip.width - rect.x);
            rect.height = Mathf.Max(1f, Mathf.Min(rect.height, clip.height - rect.y));
            content.width = rect.width - Mathf.Max(16f, GUI.skin.verticalScrollbar.fixedWidth);
            _modeScrollOpen = true;
        }

        internal static void EndModeScroll()
        {
            if (!_modeScrollOpen) return;

            _modeScrollOpen = false;
            GUI.EndGroup();
        }

        internal static void Draw(LobbyMenu menu)
        {
            bool active = _menu && _menu.Pointer == menu.Pointer && _panel.width > 0f;
            _menu = null;

            if (!active) return;

            float row = Mathf.Max(13, Screen.width / 70) * 1.8f;
            float padding = row / 1.8f * 0.7f;
            float y = _bottom - _viewport.y + padding;
            float height = GameModeMenu.SettingsHeight(menu, _width, row, padding);
            float contentHeight = y + (_sectionY < 0f ? height : 0f);
            _scroll.y = Mathf.Clamp(_scroll.y, 0f, Mathf.Max(0f, contentHeight - _viewport.height));
            _scroll = GUI.BeginScrollView
            (
                _viewport,
                _scroll,
                new Rect(0f, 0f, _width, contentHeight),
                false,
                true,
                GUI.skin.horizontalScrollbar,
                GUI.skin.verticalScrollbar,
                GUIStyle.none
            );

            try
            {
                float sectionY = _sectionY < 0f ? y : _sectionY - _viewport.y;
                GameModeMenu.DrawSettings(menu, new Rect(0f, sectionY, _width, height), row, padding);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }

        internal static void End()
        {
            _menu = null;
            EndModeScroll();
        }
    }

    [HarmonyPatch(typeof(LobbyMenu), "AJKAOGDBDMH")]
    internal static class InlineSettingsPatch
    {
        [HarmonyPrefix, HarmonyPriority(Priority.Last)]
        private static void Prefix(LobbyMenu __instance) => GameSettingsPanel.Begin(__instance);

        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        private static void Postfix(LobbyMenu __instance) => GameSettingsPanel.Draw(__instance);

        [HarmonyFinalizer]
        private static void Finalizer() => GameSettingsPanel.End();
    }

    [HarmonyPatch(typeof(GUI), nameof(GUI.Box), [typeof(Rect), typeof(string)])]
    internal static class SettingsPanelPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Rect position) => GameSettingsPanel.Capture(position);
    }

    [HarmonyPatch]
    internal static class SettingsControlsPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GUI), nameof(GUI.Button), [typeof(Rect), typeof(GUIContent), typeof(GUIStyle)]);
            yield return AccessTools.Method(typeof(GUI), nameof(GUI.Toggle), [typeof(Rect), typeof(bool), typeof(string)]);
            yield return AccessTools.Method(typeof(GUI), nameof(GUI.HorizontalSlider), [typeof(Rect), typeof(float), typeof(float), typeof(float)]);
            yield return AccessTools.Method(typeof(GUI), nameof(GUI.Label), [typeof(Rect), typeof(string), typeof(GUIStyle)]);
            yield return AccessTools.Method(typeof(GUI), nameof(GUI.TextField), [typeof(Rect), typeof(string), typeof(int), typeof(GUIStyle)]);
        }

        [HarmonyPrefix]
        private static void Prefix(ref Rect position, out bool __state, MethodBase __originalMethod) =>
            GameSettingsPanel.Clip(ref position, out __state, __originalMethod.Name == nameof(GUI.Toggle));

        [HarmonyFinalizer]
        private static void Finalizer(bool __state) => GameSettingsPanel.EndClip(__state);
    }

    [HarmonyPatch(typeof(GUI), nameof(GUI.BeginScrollView), [typeof(Rect), typeof(Vector2), typeof(Rect)])]
    internal static class ModeSettingsScrollPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref Rect position, ref Rect viewRect) => GameSettingsPanel.BeginModeScroll(ref position, ref viewRect);
    }

    [HarmonyPatch(typeof(GUI), nameof(GUI.EndScrollView), [])]
    internal static class EndModeSettingsScrollPatch
    {
        [HarmonyPostfix]
        private static void Postfix() => GameSettingsPanel.EndModeScroll();
    }
}
