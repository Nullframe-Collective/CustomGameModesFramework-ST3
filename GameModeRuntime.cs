using Il2Cpp;
using MelonLoader;

namespace CustomGameModesFramework
{
    internal static class GameModeRuntime
    {
        private static RoomMultiplayerMenu _room;
        private static bool _roundCompleted;
        private static bool _ended;
        private static bool _setup;
        private static string _failure;

        internal static CustomGameMode Active => GameModeManager.Instance.ActiveGameMode;
        internal static bool CanRun => _failure == null;

        internal static void Attach(RoomMultiplayerMenu room)
        {
            if (!room || (_room && _room.Pointer == room.Pointer)) return;

            Leave();
            _room = room;

            try
            {
                var relay = NetBootstrap.GJDGDIGECJN?.IHKEAPHEOAL;
                string data = relay != null && !relay.OfflineMode ? relay.CurrentRoom?.RoomData : null;
                data ??= ModeRoomData.Known(RoomProperties.Current);

                if (string.IsNullOrEmpty(data)) return;

                GameModeRegistration registration = ModeRoomData.Read(data, out GameModeSettings settings);

                if (registration == null) return;

                registration.Mode.Context = new GameModeContext(registration, settings, room);
                GameModeManager.Instance.ActiveGameMode = registration.Mode;
                Patches.StartPatch.GameMode = registration.Info.ModeID;
                Invoke(mode => mode.OnStart(room));
            }
            catch (Exception exception)
            {
                _failure = exception.Message;
                GameModeMenu.ShowError(exception.Message);
                MelonLogger.Error($"Cannot start custom game mode: {exception.Message}");
            }
        }

        internal static void Update()
        {
            if (_failure == null || !_room) return;

            // A private room's metadata may only arrive after JoinRoom has already succeeded.
            // Leave on the next update rather than playing the native base mode by accident.
            _room.QuitRoom();
            Leave();
        }

        internal static void Setup(RoomMultiplayerMenu room)
        {
            Attach(room);

            if (_setup) return;

            _setup = true;
            Invoke(mode => mode.OnSetupRoom(room));
        }

        internal static void StartGame(RoomMultiplayerMenu room)
        {
            Attach(room);

            if (_roundCompleted)
            {
                Invoke(mode => mode.OnRestart(room));
            }

            _roundCompleted = false;
            _ended = false;
            Invoke(mode => mode.OnStartGame(room));
        }

        internal static void Tick(RoomMultiplayerMenu room)
        {
            Attach(room);
            bool ended = room.DAEFCOIPNCP;

            if (ended && !_ended)
            {
                _roundCompleted = true;
                Invoke(mode => mode.OnRoundEnded(room));
            }

            _ended = ended;
            Invoke(mode => mode.OnFixedUpdate(room));
        }

        internal static void Leave(RoomMultiplayerMenu room = null)
        {
            if (room != null && _room != null && room.Pointer != _room.Pointer) return;

            CustomGameMode mode = Active;
            RoomMultiplayerMenu previous = _room;
            GameModeManager.Instance.ActiveGameMode = null;
            _room = null;
            _roundCompleted = _ended = _setup = false;
            _failure = null;
            Patches.StartPatch.GameMode = null;

            if (mode == null) return;

            try
            {
                mode.OnQuitRoom(previous);
            }
            catch (Exception exception)
            {
                MelonLogger.Error($"{mode.GetType().Name}.OnQuitRoom: {exception.Message}");
            }
            finally
            {
                mode.Context = null;
            }
        }

        internal static void Invoke(Action<CustomGameMode> callback)
        {
            CustomGameMode mode = Active;

            if (mode == null) return;

            try
            {
                callback(mode);
            }
            catch (Exception exception)
            {
                // ExitGUI must reach Unity so that IMGUI can finish the current event correctly.
                if (exception.GetType().Name.Contains("ExitGUI")) throw;

                MelonLogger.Error($"{mode.GetType().Name}: {exception.Message}");
            }
        }
    }
}
