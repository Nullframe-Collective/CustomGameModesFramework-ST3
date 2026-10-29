using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using UnityEngine;

namespace CustomGameModesFramework
{
    internal static class GameModeMenu
    {
        private static LobbyMenu _menu;
        private static int _revision = -1;
        private static string _error;
        private static GUISkin _styleSkin;
        private static int _fontSize;
        private static GUIStyle _label;
        private static GUIStyle _button;
        private static GUIStyle _input;
        private static GUIStyle _title;

        internal static void Invalidate()
        {
            _revision = -1;
            if (!_menu)
            {
                _menu = UnityEngine.Object.FindObjectsOfType<LobbyMenu>(true).FirstOrDefault();
            }

            Refresh();
        }

        internal static void Attach(LobbyMenu menu)
        {
            GameModeRuntime.Leave();
            _menu = menu;
            GameSettingsIntegration.Initialize();
            _revision = -1;
            Refresh();
        }

        internal static void Refresh()
        {
            if (!_menu || _menu.MHDLCNKEEGN == null || _menu.NMEONIIJMNO == null || _revision == GameModeManager.Instance.Revision) return;

            var entries = _menu.MHDLCNKEEGN;
            string selected = _menu.NNKFEAPBLCK >= 0 && _menu.NNKFEAPBLCK < entries.Count ? entries[_menu.NNKFEAPBLCK].modeID : null;

            RemoveCustomModes(entries);

            if (GameModeCatalog.Enabled)
            {
                foreach (GameModeRegistration registration in GameModeManager.Instance.Registrations)
                {
                    AddMode(registration);
                }
            }

            RestoreSelection(entries, selected);
            CopySizes();
            _revision = GameModeManager.Instance.Revision;
        }

        private static void RemoveCustomModes(Il2CppSystem.Collections.Generic.List<LobbyMenu.AllModes> entries)
        {
            for (int index = entries.Count - 1; index >= 0; index--)
            {
                if (!GameModeManager.Instance.IsRegistered(entries[index].modeID)) continue;

                entries.RemoveAt(index);

                foreach (LobbyMenu.AllSizes size in _menu.NMEONIIJMNO)
                {
                    string[] counts = size.playerCount?.ToArray() ?? Array.Empty<string>();
                    size.playerCount = new Il2CppStringArray(counts.Where((_, position) => position != index).ToArray());
                }
            }
        }

        private static void AddMode(GameModeRegistration registration)
        {
            var modes = _menu.MHDLCNKEEGN;
            int index = -1;
            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i].modeID == registration.Info.ModeID)
                {
                    index = i;
                }
            }

            if (index < 0)
            {
                index = modes.Count;
                modes.Add(new LobbyMenu.AllModes
                {
                    modeID = registration.Info.ModeID,
                    modeName = registration.Info.ModeName,
                    players = registration.Info.Players
                });
            }

            foreach (LobbyMenu.AllSizes size in _menu.NMEONIIJMNO)
            {
                string[] counts = size.playerCount?.ToArray() ?? Array.Empty<string>();
                if (counts.Length <= index)
                {
                    Array.Resize(ref counts, index + 1);
                }

                counts[index] = size.sizeName switch
                {
                    "Small" => registration.Info.SmallMapPlayerCount,
                    "Medium" => registration.Info.MediumMapPlayerCount,
                    "Large" => registration.Info.LargeMapPlayerCount,
                    _ => registration.Info.Players
                };

                size.playerCount = new Il2CppStringArray(counts);
            }
        }

        private static void RestoreSelection(Il2CppSystem.Collections.Generic.List<LobbyMenu.AllModes> entries, string selected)
        {
            _menu.NNKFEAPBLCK = 0;
            for (int index = 0; index < entries.Count; index++)
            {
                if (entries[index].modeID == selected)
                {
                    _menu.NNKFEAPBLCK = index;
                }
            }
        }

        private static void CopySizes()
        {
            Patches.LobbyMenuOptionsPatch.AllSizes.Clear();
            foreach (LobbyMenu.AllSizes size in _menu.NMEONIIJMNO)
            {
                Patches.LobbyMenuOptionsPatch.AllSizes.Add(new Patches.AllSizes
                {
                    sizeName = size.sizeName,
                    playerCount = size.playerCount.ToArray().ToList()
                });
            }
        }

        internal static GameModeRegistration Selected(LobbyMenu menu)
        {
            if (!GameModeCatalog.Enabled || !menu || menu.MHDLCNKEEGN == null) return null;
            if (menu.NNKFEAPBLCK < 0 || menu.NNKFEAPBLCK >= menu.MHDLCNKEEGN.Count) return null;

            return GameModeManager.Instance.Find(menu.MHDLCNKEEGN[menu.NNKFEAPBLCK].modeID);
        }

        internal static void Draw(LobbyMenu menu)
        {
            if (!menu || menu.EJLDOIOJGPC || !menu.FANLKBJODCL) return;

            GameModeRegistration selected = Selected(menu);
            if (selected == null) return;

            foreach (Action action in GameModeManager.Instance.GetActionsForLobbyMenuOptions(menu))
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    // ExitGUI must reach Unity so that IMGUI can finish the current event correctly.
                    if (exception.GetType().Name.Contains("ExitGUI")) throw;

                    MelonLogger.Error($"{selected.Info.ModeID}.LobbyMenuOptions: {exception.Message}");
                }
            }
        }

        internal static float SettingsHeight(LobbyMenu menu, float width, float row, float padding)
        {
            GameModeRegistration registration = Selected(menu);

            if (registration == null || registration.Settings.Entries.Count == 0) return 0f;

            return row * (1.35f + registration.Settings.Entries.Sum(setting =>
                setting.Kind == GameModeSettingKind.Number ? 2f : 1.35f)) + padding;
        }

        internal static void DrawSettings(LobbyMenu menu, Rect area, float row, float padding)
        {
            GameModeRegistration registration = Selected(menu);
            if (registration == null || registration.Settings.Entries.Count == 0 || menu.EGDMPMDBJIK) return;

            GUISkin previous = GUI.skin;
            Color color = GUI.color;

            try
            {
                GUI.skin = menu.BIPMFIBNFBC;
                GUI.color = Color.white;
                PrepareStyles(Math.Max(18, Screen.width / 55));

                float width = area.width - padding * 2f;
                float y = area.y;
                float x = area.x + padding;
                GUI.Label(new Rect(x, y, width, row), registration.Info.ModeName, _title);
                y += row * 1.35f;

                foreach (GameModeSetting setting in registration.Settings.Entries)
                {
                    DrawSetting(registration, setting, x, y, width, row);
                    y += row * (setting.Kind == GameModeSettingKind.Number ? 2f : 1.35f);
                }
            }
            finally
            {
                GUI.skin = previous;
                GUI.color = color;
            }
        }

        private static void DrawSetting(GameModeRegistration registration, GameModeSetting setting, float x, float y, float width, float row)
        {
            Rect label = new(x, y, width * 0.48f, row);
            Rect control = new(x + width * 0.52f, y, width * 0.48f, row);
            GUI.SetNextControlName("GameMode:" + registration.Info.ModeID + ":" + setting.Id);

            switch (setting.Kind)
            {
                case GameModeSettingKind.Toggle:
                    GUI.Label(label, setting.Label, _label);
                    if (GUI.Button(control, (bool)setting.Value ? "On" : "Off", _button))
                    {
                        registration.Settings.Set(setting.Id, !(bool)setting.Value);
                    }

                    break;
                case GameModeSettingKind.Number:
                    GUI.Label(new Rect(x, y, width, row), setting.Label + ": " + ((float)setting.Value).ToString("0.##"), _label);
                    float value = GUI.HorizontalSlider(new Rect(x, y + row, width, row * 0.6f), (float)setting.Value, setting.Minimum, setting.Maximum);
                    registration.Settings.Set(setting.Id, value);
                    break;
                case GameModeSettingKind.Choice:
                    GUI.Label(label, setting.Label, _label);
                    if (GUI.Button(control, (string)setting.Value, _button))
                    {
                        int index = setting.Choices.ToList().IndexOf((string)setting.Value);
                        registration.Settings.Set(setting.Id, setting.Choices[(index + 1) % setting.Choices.Count]);
                    }

                    break;
                case GameModeSettingKind.Text:
                    GUI.Label(label, setting.Label, _label);
                    registration.Settings.Set(setting.Id, GUI.TextField(control, (string)setting.Value, setting.MaxLength, _input));
                    break;
            }
        }

        internal static void ShowError(string error)
        {
            _error = error;
            MelonLogger.Warning(error);
        }

        private static void PrepareStyles(int size = 0)
        {
            if (size == 0)
            {
                size = Math.Max(18, Mathf.RoundToInt(Screen.height * 0.03f));
            }

            if (_styleSkin == GUI.skin && _fontSize == size) return;

            _styleSkin = GUI.skin;
            _fontSize = size;
            GUIStyle nativeLabel = GUI.skin.customStyles.Length > 0 ? GUI.skin.customStyles[0] : GUI.skin.label;
            Font font = nativeLabel.font ? nativeLabel.font : GUI.skin.font;
            _label = new GUIStyle(nativeLabel) { font = font, fontSize = size, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            _button = new GUIStyle(GUI.skin.button) { font = font, fontSize = size };
            _input = new GUIStyle(GUI.skin.textField) { font = font, fontSize = size };
            _title = new GUIStyle(_label) { fontSize = size + 3, alignment = TextAnchor.MiddleCenter };
        }

        internal static void DrawError()
        {
            if (_error == null) return;

            PrepareStyles();
            Rect panel = new(Screen.width * 0.2f, Screen.height * 0.3f, Screen.width * 0.6f, Screen.height * 0.4f);
            GUI.DrawTexture(panel, Texture2D.blackTexture);
            GUI.Box(panel, "Custom Game Mode");
            GUI.Label(new Rect(panel.x + 25, panel.y + 45, panel.width - 50, panel.height - 110), _error, _label);

            if (GUI.Button(new Rect(panel.x + 25, panel.yMax - 55, panel.width - 50, 35), "Close"))
            {
                _error = null;
            }
        }

        internal static void Close(LobbyMenu menu)
        {
            if (_menu == null || _menu.Pointer != menu.Pointer) return;

            _menu = null;
        }
    }
}
