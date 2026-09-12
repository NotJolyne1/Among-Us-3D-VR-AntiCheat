using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;

[HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_OnEnterCustomization))]
public static class OnEnterCustomizationPatch
{
    public static void Prefix(NetworkedLocomotionPlayer __instance)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled && Settings.CosmeticValidateModule)
        {
            if (!GameReferences.GameState!.InLobbyState() && !__instance.PState.IsSpectating)
            {
                AntiCheatMain.Detected(__instance.PState, "Illegal customization entry");
            }
        }
    }
}