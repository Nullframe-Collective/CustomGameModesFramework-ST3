using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Il2Cpp;

namespace CustomGameModesFramework
{
    internal static class ModeRoomData
    {
        internal const int MaximumBytes = 1024;
        private static readonly Dictionary<IntPtr, (RoomProperties Properties, string Data)> _properties = [];

        internal static JsonObject Parse(string data) => JsonNode.Parse(data, documentOptions:
            new JsonDocumentOptions { MaxDepth = 12 }) as JsonObject ?? throw new ArgumentException("Invalid room data.");

        internal static string Write(string data, GameModeRegistration registration, GameModeSettings settings)
        {
            if (!GameModeCatalog.Enabled) throw new ArgumentException("Enable CustomGameModesFramework in ADDONS before using a custom mode.");

            JsonObject room = Parse(data);
            room["GameMode"] = registration.Info.BaseMode;
            room["CustomMode"] = new JsonObject
            {
                ["Protocol"] = 1,
                ["Id"] = registration.Info.ModeID,
                ["Name"] = registration.Info.ModeName,
                ["Options"] = settings.ToJson()
            };

            return room.ToJsonString();
        }

        internal static GameModeRegistration Read(string data, out GameModeSettings settings)
        {
            settings = null;
            JsonObject room = Parse(data);

            if (!room.ContainsKey("CustomMode")) return null;

            if (!GameModeCatalog.Enabled) throw new ArgumentException("Enable CustomGameModesFramework in ADDONS before joining this server.");

            JsonObject custom = room["CustomMode"] as JsonObject ?? throw new ArgumentException("Invalid custom mode data.");

            if (custom["Protocol"]?.GetValue<int>() != 1)
            {
                throw new ArgumentException("The host uses an unsupported game mode protocol.");
            }

            string id = custom["Id"]?.GetValue<string>();
            GameModeRegistration registration = GameModeManager.Instance.Find(id) ??
                throw new ArgumentException($"Install the host's game mode: {id}.");

            if (room["GameMode"]?.GetValue<string>() != registration.Info.BaseMode)
            {
                throw new ArgumentException($"The base rules of game mode {id} do not match this server.");
            }

            JsonObject options = custom["Options"] as JsonObject ?? throw new ArgumentException("Missing mode settings.");
            settings = registration.Settings.Snapshot(options);

            return registration;
        }

        internal static void CheckSize(string data)
        {
            if (Encoding.UTF8.GetByteCount(data) > MaximumBytes)
            {
                throw new ArgumentException("Room settings exceed 1024 bytes. Shorten text options or reduce the mode's settings.");
            }
        }

        internal static void Prepare(RoomProperties properties)
        {
            if (properties == null) return;

            GameModeRegistration registration = GameModeManager.Instance.Find(properties.GameMode);

            if (registration == null) return;

            // Native spawning, teams and respawning still need a game mode understood by the game.
            string data = Write(properties.NMLMDLKKFCJ(), registration, registration.Settings);
            properties.GameMode = registration.Info.BaseMode;
            Remember(properties, data);
        }

        internal static void Remember(RoomProperties properties, string data)
        {
            if (properties == null) return;

            if (_properties.Count >= 256)
            {
                _properties.Clear();
            }

            _properties[properties.Pointer] = (properties, data);
        }

        internal static string Known(RoomProperties properties) => properties != null &&
            _properties.TryGetValue(properties.Pointer, out var entry) ? entry.Data : null;

        internal static string Serialize(RoomProperties properties, string original)
        {
            GameModeRegistration registration = GameModeManager.Instance.Find(properties.GameMode);

            if (registration != null) return Write(original, registration, registration.Settings);

            string known = Known(properties);

            if (known == null) return original;

            JsonObject room = Parse(original);
            JsonObject remembered = Parse(known);

            if (remembered["CustomMode"] is JsonObject custom &&
                room["GameMode"]?.GetValue<string>() == remembered["GameMode"]?.GetValue<string>())
            {
                room["CustomMode"] = JsonNode.Parse(custom.ToJsonString());
            }

            return room.ToJsonString();
        }
    }
}
