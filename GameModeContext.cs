using Il2Cpp;
using Il2CppFishNet.Managing;
using Il2CppFishNet.Transporting.Relay;

namespace CustomGameModesFramework
{
    public sealed class GameModeContext
    {
        public string ModeID { get; }
        public string BaseMode { get; }
        public GameModeSettings Settings { get; }
        public RoomMultiplayerMenu Room { get; }
        public NetworkManager Network => NetBootstrap.GJDGDIGECJN?.HLNKMGIBPIG;
        public RelayTransport Relay => NetBootstrap.GJDGDIGECJN?.IHKEAPHEOAL;
        public bool IsHost => Relay != null && (Relay.OfflineMode || Relay.IsRoomHost);

        internal GameModeContext(GameModeRegistration registration, GameModeSettings settings, RoomMultiplayerMenu room)
        {
            ModeID = registration.Info.ModeID;
            BaseMode = registration.Info.BaseMode;
            Settings = settings;
            Room = room;
        }

        public void SpawnLocalPlayer(string teamName = "")
        {
            if (Room)
            {
                Room.SpawnPlayer(teamName);
            }
        }
    }
}
