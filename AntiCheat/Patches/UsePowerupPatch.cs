using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;

[HarmonyPatch(typeof(PlayerState), nameof(PlayerState.RPC_UsePowerUp))]
public static class UsePowerupPatch
{
    public static bool Prefix(PlayerState __instance)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyUsePowerup(__instance))
            {
                MelonLogger.Warning("Someone in your lobby is cheating! Reason: UsePowerup called illegally");
                return false;
            }
        }
        return true;
    }
}