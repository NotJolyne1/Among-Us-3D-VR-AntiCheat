using AntiCheat.Config;
using AntiCheat.Managers;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using MelonLoader;

[HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.RPC_ToggleLobbyDoors))]
public static class ToggleLobbyDoorsPatch
{
    [HarmonyPostfix]
    public static void Prefix(GameStateManager __instance, NetworkBool close)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyToggleLobbyDoors(__instance, close))
            {
                MelonLogger.Warning("Someone in your lobby is cheating. Reason: Lobby doors illegally toggled");
                MelonCoroutines.Start(Commands.CorrectLobbyDoors(__instance));
            }
        }
    }


}