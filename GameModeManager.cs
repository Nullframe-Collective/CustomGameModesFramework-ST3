using Il2Cpp;
using System.Reflection;

namespace CustomGameModesFramework
{
    public class GameModeManager
    {
        public static GameModeManager Instance { get; } = new GameModeManager();
        public List<CustomGameMode> _gameModes = [];
        public List<GameModeInfo> GameModeInfos = [];

        private readonly Dictionary<string, GameModeRegistration> _registrations = new(StringComparer.Ordinal);

        public CustomGameMode ActiveGameMode { get; internal set; }
        internal IEnumerable<GameModeRegistration> Registrations => _registrations.Values;
        internal int Revision { get; private set; }

        public void RegisterGameMode(CustomGameMode customGameMode)
        {
            ArgumentNullException.ThrowIfNull(customGameMode);

            Type type = customGameMode.GetType();
            GameModeInfo info = type.GetCustomAttribute<GameModeInfo>() ??
                throw new ArgumentException($"{type.FullName} requires GameModeInfo.");

            if (!GameModeSettings.ValidId(info.ModeID) || GameModeInfo.NativeModes.Contains(info.ModeID))
            {
                throw new ArgumentException($"Invalid or reserved mode ID: {info.ModeID}.");
            }

            if (string.IsNullOrWhiteSpace(info.ModeName) || !GameModeInfo.NativeModes.Contains(info.BaseMode))
            {
                throw new ArgumentException($"{info.ModeID} requires a name and a supported BaseMode.");
            }

            if (!int.TryParse(info.Players, out int players) || players < 1 || players > 32)
            {
                throw new ArgumentException($"{info.ModeID}.Players must be a player limit between 1 and 32, for example \"12\".");
            }

            if (_registrations.TryGetValue(info.ModeID, out GameModeRegistration existing))
            {
                if (existing.Mode.GetType() == type) return;

                throw new ArgumentException($"Mode ID {info.ModeID} is already registered by {existing.Mode.GetType().FullName}.");
            }

            GameModeSettings settings = new();
            customGameMode.ConfigureSettings(settings);
            settings.Seal();

            _registrations.Add(info.ModeID, new GameModeRegistration(customGameMode, info, settings));
            _gameModes.Add(customGameMode);
            GameModeInfos.Add(info);
            Revision++;
        }

        public bool IsRegistered(string modeID) => modeID != null && _registrations.ContainsKey(modeID);

        internal GameModeRegistration Find(string modeID) =>
            modeID != null && _registrations.TryGetValue(modeID, out GameModeRegistration registration) ? registration : null;

        private IEnumerable<Action> Actions(Action<CustomGameMode> action)
        {
            CustomGameMode mode = ActiveGameMode;

            if (mode != null) yield return () => action(mode);
        }

        public IEnumerable<Action> GetActionsForStart(RoomMultiplayerMenu instance) => Actions(mode => mode.OnStart(instance));
        public IEnumerable<Action> GetActionsForSetupRoom(RoomMultiplayerMenu instance) => Actions(mode => mode.OnSetupRoom(instance));
        public IEnumerable<Action> GetActionsForStartGame(RoomMultiplayerMenu instance) => Actions(mode => mode.OnStartGame(instance));
        public IEnumerable<Action> GetActionsForOnGUI(RoomMultiplayerMenu instance) => Actions(mode => mode.OnGUI(instance));
        public IEnumerable<Action> GetActionsForFixedUpdate(RoomMultiplayerMenu instance) => Actions(mode => mode.OnFixedUpdate(instance));
        public IEnumerable<Action> GetActionsForSpawnPlayer(RoomMultiplayerMenu instance) => Actions(mode => mode.OnSpawnPlayer(instance));
        public IEnumerable<Action> GetActionsForQuitRoom(RoomMultiplayerMenu instance) => Actions(mode => mode.OnQuitRoom(instance));
        public IEnumerable<Action> GetActionsForRoundEnded(RoomMultiplayerMenu instance) => Actions(mode => mode.OnRoundEnded(instance));
        public IEnumerable<Action> GetActionsForRestart(RoomMultiplayerMenu instance) => Actions(mode => mode.OnRestart(instance));
        public IEnumerable<Action> GetActionsForRespawnPlayerGUI(RagdollController instance) => Actions(mode => mode.OnRespawnPlayerGUI(instance));
        public IEnumerable<Action> GetActionsForRespawnPlayer(RagdollController instance) => Actions(mode => mode.OnRespawnPlayer(instance));
        public IEnumerable<Action> GetActionsForLobbyMenuOptions(LobbyMenu instance)
        {
            CustomGameMode mode = GameModeMenu.Selected(instance)?.Mode;

            if (mode != null) yield return () => mode.LobbyMenuOptions(instance);
        }
    }

    internal sealed class GameModeRegistration
    {
        internal CustomGameMode Mode { get; }
        internal GameModeInfo Info { get; }
        internal GameModeSettings Settings { get; }

        internal GameModeRegistration(CustomGameMode mode, GameModeInfo info, GameModeSettings settings)
        {
            Mode = mode;
            Info = info;
            Settings = settings;
        }
    }
}
