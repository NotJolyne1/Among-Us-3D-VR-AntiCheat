using AntiCheat;
using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using static MelonLoader.MelonLogger;

[HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_ApplyElement))]
public static class ApplyElementPatch
{
    [HarmonyPrefix]
    public static bool Prefix(NetworkedLocomotionPlayer __instance, int elementIndex, int playerIndex)
    {

        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyCosmeticChange(__instance, elementIndex, playerIndex))
            {
                AntiCheatMain.Detected(__instance.PState, "Illegal cosmetic data");
                return false;
            }
        }
        return true;
    }
}