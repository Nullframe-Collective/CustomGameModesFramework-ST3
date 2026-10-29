using System.Text.Json.Nodes;

namespace CustomGameModesFramework
{
    public sealed class GameModeSettings
    {
        private readonly List<GameModeSetting> _entries = [];
        private bool _sealed;

        public IReadOnlyList<GameModeSetting> Entries => _entries.AsReadOnly();
        public bool IsReadOnly { get; private set; }

        public void Toggle(string id, string label, bool defaultValue = false) =>
            Add(new GameModeSetting(id, label, GameModeSettingKind.Toggle, defaultValue));

        public void Number(string id, string label, float defaultValue, float minimum, float maximum)
        {
            if (!float.IsFinite(minimum) || !float.IsFinite(maximum) || minimum >= maximum ||
                !float.IsFinite(defaultValue) || defaultValue < minimum || defaultValue > maximum)
            {
                throw new ArgumentException($"Invalid range for setting {id}.");
            }

            Add(new GameModeSetting(id, label, GameModeSettingKind.Number, defaultValue, minimum, maximum));
        }

        public void Choice(string id, string label, string defaultValue, params string[] choices)
        {
            if (choices == null || choices.Length < 1 || choices.Length > 32 ||
                choices.Any(value => value == null || value.Length > 80) ||
                choices.Distinct(StringComparer.Ordinal).Count() != choices.Length || !choices.Contains(defaultValue))
            {
                throw new ArgumentException($"Invalid choices for setting {id}.");
            }

            Add(new GameModeSetting(id, label, GameModeSettingKind.Choice, defaultValue, choices: choices.ToArray()));
        }

        public void Text(string id, string label, string defaultValue = "", int maxLength = 80)
        {
            if (maxLength < 1 || maxLength > 160 || defaultValue == null || defaultValue.Length > maxLength)
            {
                throw new ArgumentException($"Invalid text setting {id}.");
            }

            Add(new GameModeSetting(id, label, GameModeSettingKind.Text, defaultValue, maxLength: maxLength));
        }

        public bool GetBool(string id) => (bool)Find(id).Value;
        public float GetNumber(string id) => (float)Find(id).Value;
        public string GetString(string id) => (string)Find(id).Value;

        public void Set(string id, object value)
        {
            if (IsReadOnly) throw new InvalidOperationException("Room settings are controlled by the host.");

            GameModeSetting entry = Find(id);
            entry.Value = entry.Validate(value);
        }

        internal static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id) && id.Length <= 64 &&
            id.All(character => (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') ||
                (character >= '0' && character <= '9') || character is '_' or '-' or ':' or '.');

        private void Add(GameModeSetting entry)
        {
            if (_sealed) throw new InvalidOperationException("Declare settings in ConfigureSettings.");

            if (!ValidId(entry.Id) || string.IsNullOrWhiteSpace(entry.Label) || _entries.Count >= 24 ||
                _entries.Any(existing => existing.Id == entry.Id))
            {
                throw new ArgumentException($"Invalid or duplicate setting ID: {entry.Id}.");
            }

            _entries.Add(entry);
        }

        private GameModeSetting Find(string id) => _entries.FirstOrDefault(entry => entry.Id == id) ??
            throw new KeyNotFoundException($"Unknown setting: {id}.");

        internal void Seal() => _sealed = true;

        internal GameModeSettings Snapshot(JsonObject options = null)
        {
            GameModeSettings copy = new();

            foreach (GameModeSetting entry in _entries)
            {
                GameModeSetting item = entry.Copy();
                item.Value = item.DefaultValue;

                if (options?.TryGetPropertyValue(item.Id, out JsonNode value) == true)
                {
                    item.Value = item.ValidateJson(value);
                }

                copy._entries.Add(item);
            }

            if (options != null && (options.Count != _entries.Count ||
                options.Any(option => !_entries.Any(entry => entry.Id == option.Key))))
            {
                throw new ArgumentException("The host uses settings that this version of the mode does not support.");
            }

            copy.Seal();
            copy.IsReadOnly = true;

            return copy;
        }

        internal JsonObject ToJson()
        {
            JsonObject result = new();

            foreach (GameModeSetting entry in _entries)
            {
                result[entry.Id] = entry.Kind switch
                {
                    GameModeSettingKind.Toggle => JsonValue.Create((bool)entry.Value),
                    GameModeSettingKind.Number => JsonValue.Create((float)entry.Value),
                    _ => JsonValue.Create((string)entry.Value)
                };
            }

            return result;
        }
    }

    public enum GameModeSettingKind
    {
        Toggle,
        Number,
        Choice,
        Text
    }

    public sealed class GameModeSetting
    {
        public string Id { get; }
        public string Label { get; }
        public GameModeSettingKind Kind { get; }
        public object Value { get; internal set; }
        public object DefaultValue { get; }
        public float Minimum { get; }
        public float Maximum { get; }
        public IReadOnlyList<string> Choices { get; }
        public int MaxLength { get; }

        internal GameModeSetting(string id, string label, GameModeSettingKind kind, object defaultValue,
            float minimum = 0, float maximum = 0, string[] choices = null, int maxLength = 0)
        {
            Id = id;
            Label = label;
            Kind = kind;
            Value = DefaultValue = defaultValue;
            Minimum = minimum;
            Maximum = maximum;
            Choices = Array.AsReadOnly(choices ?? Array.Empty<string>());
            MaxLength = maxLength;
        }

        internal GameModeSetting Copy() => new(Id, Label, Kind, DefaultValue, Minimum, Maximum, Choices.ToArray(), MaxLength);

        internal object Validate(object value)
        {
            if (Kind == GameModeSettingKind.Toggle && value is bool toggle) return toggle;

            if (Kind == GameModeSettingKind.Number && value is float number && float.IsFinite(number) &&
                number >= Minimum && number <= Maximum) return number;

            if (value is string text && ((Kind == GameModeSettingKind.Text && text.Length <= MaxLength) ||
                (Kind == GameModeSettingKind.Choice && Choices.Contains(text)))) return text;

            throw new ArgumentException($"Invalid value for setting {Id}.");
        }

        internal object ValidateJson(JsonNode value)
        {
            if (value is not JsonValue json) throw new ArgumentException($"Invalid setting {Id}.");

            return Kind switch
            {
                GameModeSettingKind.Toggle => Validate(json.GetValue<bool>()),
                GameModeSettingKind.Number => Validate(json.GetValue<float>()),
                _ => Validate(json.GetValue<string>())
            };
        }
    }
}
