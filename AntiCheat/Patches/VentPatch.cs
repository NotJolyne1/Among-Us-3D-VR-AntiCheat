using AntiCheat;
using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock.Network;

[HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_EnterVent))]
public static class EnterVentPatch
{
    [HarmonyPrefix]
    public static bool Prefix(NetworkedLocomotionPlayer __instance)
    {

        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyVentEnter(__instance))
            {
                AntiCheatMain.CheaterDetected(__instance.PState, "Illegal vent enter data");
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_ExitVent))]
public static class ExitVentPatch
{
    [HarmonyPrefix]
    public static bool Prefix(NetworkedLocomotionPlayer __instance)
    {

        if (Settings.IsHost && Settings.AntiCheatEnabled && Settings.VentValidateModule)
        {
            if (!AntiCheatMain.VerifyVentExit(__instance))
            {
                AntiCheatMain.CheaterDetected(__instance.PState, "Illegal vent exit data");
                return false;
            }
        }
        return true;
    }
}