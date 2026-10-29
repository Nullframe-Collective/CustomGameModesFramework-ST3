using Il2Cpp;

namespace CustomGameModesFramework
{
    public abstract class CustomGameMode
    {
        public GameModeContext Context { get; internal set; }
        public GameModeSettings Settings => Context?.Settings;

        public virtual bool UseNativeHUD => true;
        public virtual bool UseNativeRespawnGUI => true;

        public virtual void ConfigureSettings(GameModeSettings settings) { }

        public virtual void OnStart(RoomMultiplayerMenu instance) { }
        public virtual void OnSetupRoom(RoomMultiplayerMenu instance) { }
        public virtual void OnStartGame(RoomMultiplayerMenu instance) { }
        public virtual void OnGUI(RoomMultiplayerMenu instance) { }
        public virtual void OnFixedUpdate(RoomMultiplayerMenu instance) { }
        public virtual void OnSpawnPlayer(RoomMultiplayerMenu instance) { }
        public virtual void OnQuitRoom(RoomMultiplayerMenu instance) { }
        public virtual void OnRoundEnded(RoomMultiplayerMenu instance) { }
        public virtual void OnRestart(RoomMultiplayerMenu instance) { }
        public virtual void OnRespawnPlayerGUI(RagdollController instance) { }
        public virtual void OnRespawnPlayer(RagdollController instance) { }
        public virtual void LobbyMenuOptions(LobbyMenu instance) { }
    }
}
