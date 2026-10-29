using HarmonyLib;
using Il2Cpp;
using Il2CppFishNet.Transporting.Relay;
using System.Reflection;

namespace CustomGameModesFramework.Patches
{
    public class AllSizes
    {
        public string sizeName;
        public List<string> playerCount;
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), nameof(RoomMultiplayerMenu.Start))]
    public static class StartPatch
    {
        public static string GameMode;

        [HarmonyPrefix]
        public static bool Prefix(RoomMultiplayerMenu __instance)
        {
            GameModeRuntime.Attach(__instance);

            return GameModeRuntime.CanRun;
        }
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), "LOHLFHPLKKD")]
    public static class SetupRoomPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(RoomMultiplayerMenu __instance)
        {
            GameModeRuntime.Attach(__instance);

            return GameModeRuntime.CanRun;
        }

        [HarmonyPostfix]
        public static void Postfix(RoomMultiplayerMenu __instance) => GameModeRuntime.Setup(__instance);
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), "INBHDPAHOGC")]
    public static class StartGamePatch
    {
        [HarmonyPostfix]
        public static void Postfix(RoomMultiplayerMenu __instance) => GameModeRuntime.StartGame(__instance);
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), nameof(RoomMultiplayerMenu.OnGUI))]
    public static class OnGUIPatch
    {
        public static bool EnableTimer;
        public static bool ShowLeaderText;
        public static bool ShouldShowWaitingText;
        public static bool IsWaitingForPlayers;
        public static string RoundEndedText = string.Empty;

        [HarmonyPrefix]
        public static bool Prefix() => GameModeRuntime.Active?.UseNativeHUD != false;

        [HarmonyPostfix]
        public static void Postfix(RoomMultiplayerMenu __instance) => GameModeRuntime.Invoke(mode => mode.OnGUI(__instance));
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), nameof(RoomMultiplayerMenu.FixedUpdate))]
    public static class FixedUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(RoomMultiplayerMenu __instance) => GameModeRuntime.Tick(__instance);
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), nameof(RoomMultiplayerMenu.SpawnPlayer))]
    public static class SpawnPlayerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix() => GameModeRuntime.CanRun;

        [HarmonyPostfix]
        public static void Postfix(RoomMultiplayerMenu __instance, bool __runOriginal)
        {
            if (__runOriginal)
            {
                GameModeRuntime.Invoke(mode => mode.OnSpawnPlayer(__instance));
            }
        }
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), nameof(RoomMultiplayerMenu.QuitRoom))]
    public static class QuitRoomPatch
    {
        [HarmonyPrefix]
        public static void Prefix(RoomMultiplayerMenu __instance) => GameModeRuntime.Leave(__instance);
    }

    [HarmonyPatch(typeof(RoomMultiplayerMenu), nameof(RoomMultiplayerMenu.OnDestroy))]
    internal static class RoomDestroyedPatch
    {
        [HarmonyPrefix]
        private static void Prefix(RoomMultiplayerMenu __instance) => GameModeRuntime.Leave(__instance);
    }

    [HarmonyPatch(typeof(RagdollController), nameof(RagdollController.OnGUI))]
    public static class RespawnPlayerGUIPatch
    {
        public static bool ShowRespawnText = true;

        [HarmonyPrefix]
        public static bool Prefix(RagdollController __instance) => !__instance.OGLEOGIHGLH ||
            GameModeRuntime.Active?.UseNativeRespawnGUI != false;

        [HarmonyPostfix]
        public static void Postfix(RagdollController __instance)
        {
            if (__instance.OGLEOGIHGLH)
            {
                GameModeRuntime.Invoke(mode => mode.OnRespawnPlayerGUI(__instance));
            }
        }
    }

    [HarmonyPatch(typeof(RagdollController), "BLACJABKHFN")]
    public static class RespawnPlayerPatch
    {
        [HarmonyPrefix]
        public static void Prefix(RagdollController __instance, out bool __state) =>
            __state = __instance.OGLEOGIHGLH && __instance.NADEDDKIIJL == 1;

        [HarmonyPostfix]
        public static void Postfix(RagdollController __instance, bool __state, bool __runOriginal)
        {
            if (__state && __runOriginal)
            {
                GameModeRuntime.Invoke(mode => mode.OnRespawnPlayer(__instance));
            }
        }
    }

    [HarmonyPatch(typeof(LobbyMenu), nameof(LobbyMenu.Start))]
    public static class LobbyMenuStartPatch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.First)]
        public static void Postfix(LobbyMenu __instance) => GameModeMenu.Attach(__instance);
    }

    [HarmonyPatch(typeof(LobbyMenu), nameof(LobbyMenu.OnGUI))]
    public static class LobbyMenuOptionsPatch
    {
        public static List<AllSizes> AllSizes = [];
        public static bool IsTimerOptionDisabled;

        [HarmonyPostfix, HarmonyPriority(Priority.Last)]
        public static void Postfix(LobbyMenu __instance) => GameModeMenu.Draw(__instance);
    }

    [HarmonyPatch(typeof(LobbyMenu), nameof(LobbyMenu.OnDisable))]
    internal static class LobbyClosedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(LobbyMenu __instance) => GameModeMenu.Close(__instance);
    }

    [HarmonyPatch(typeof(NetBootstrap), "MEIFJPMJHMH")]
    internal static class CreateRoomPatch
    {
        [HarmonyPrefix]
        private static void Prefix(RoomProperties __2) => ModeRoomData.Prepare(__2);
    }

    [HarmonyPatch(typeof(NetBootstrap), "DBBODPFPDDM")]
    internal static class SetRoomPatch
    {
        [HarmonyPrefix]
        private static void Prefix(RoomProperties __0) => ModeRoomData.Prepare(__0);
    }

    [HarmonyPatch(typeof(RoomProperties), "NMLMDLKKFCJ")]
    internal static class SerializeRoomPatch
    {
        [HarmonyPostfix]
        private static void Postfix(RoomProperties __instance, ref string __result) =>
            __result = ModeRoomData.Serialize(__instance, __result);
    }

    [HarmonyPatch(typeof(RoomProperties), "HFBENCJFGPO")]
    internal static class ReadRoomPatch
    {
        [HarmonyPostfix]
        private static void Postfix(string __0, RoomProperties __result)
        {
            if (__result != null)
            {
                ModeRoomData.Remember(__result, __0);
            }
        }
    }

    [HarmonyPatch]
    internal static class PublishRoomPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(RelayTransport), nameof(RelayTransport.HostRoom));
            yield return AccessTools.Method(typeof(RelayTransport), nameof(RelayTransport.UpdateRoom));
        }

        [HarmonyPrefix, HarmonyPriority(Priority.Last)]
        private static bool Prefix(RelayTransport __instance, ref string roomData, ref bool __result)
        {
            try
            {
                var room = ModeRoomData.Parse(roomData);
                GameModeRegistration registration = GameModeManager.Instance.Find(room["GameMode"]?.GetValue<string>());

                if (registration != null)
                {
                    roomData = ModeRoomData.Write(roomData, registration, registration.Settings);
                }
                else if (!room.ContainsKey("CustomMode") && __instance.CurrentRoom != null)
                {
                    var current = ModeRoomData.Parse(__instance.CurrentRoom.RoomData);

                    if (current.ContainsKey("CustomMode") && current["GameMode"]?.ToJsonString() == room["GameMode"]?.ToJsonString())
                    {
                        room["CustomMode"] = System.Text.Json.Nodes.JsonNode.Parse(current["CustomMode"].ToJsonString());
                        roomData = room.ToJsonString();
                    }
                }

                ModeRoomData.Read(roomData, out _);
                ModeRoomData.CheckSize(roomData);

                return true;
            }
            catch (Exception exception)
            {
                __result = false;
                GameModeMenu.ShowError(exception.Message);

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(RelayTransport), nameof(RelayTransport.JoinRoom))]
    internal static class JoinRoomPatch
    {
        [HarmonyPrefix, HarmonyPriority(Priority.First)]
        private static bool Prefix(string roomId, ref bool __result)
        {
            try
            {
                var rooms = NetBootstrap.GJDGDIGECJN?.NLGEHGBOCGE;

                if (rooms == null) return true;

                int count = rooms.Cast<Il2CppSystem.Collections.Generic.IReadOnlyCollection<RelayRoomInfo>>().Count;

                for (int i = 0; i < count; i++)
                {
                    if (rooms[i].Id != roomId) continue;

                    ModeRoomData.Read(rooms[i].RoomData, out _);

                    break;
                }

                return true;
            }
            catch (Exception exception)
            {
                __result = false;
                GameModeMenu.ShowError(exception.Message);

                return false;
            }
        }
    }
}
