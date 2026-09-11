using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Minigames;
using Il2CppSG.Airlock.Network;
using MelonLoader;

[HarmonyPatch(typeof(MinigameManager), nameof(MinigameManager.RPC_RequestSpecificPowerUp))]
public static class RequestSpecificPowerupPatch
{
    public static bool Prefix(int playerID, string playerFacingPowerName)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (GameReferences.GameState!.GameModeStateValue.GameMode == GameModes.Infection)
            {
                MelonLogger.Warning($"Someone in your room is cheating, unable to identify. Reason: {playerID} illegally invoked RPC_RequestSpecificPowerUp for {playerFacingPowerName}");
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(MinigameManager), nameof(MinigameManager.RPC_RequestPowerUp))]
public static class RequestPowerupPatch
{
    public static bool Prefix(int playerID)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (GameReferences.GameState!.GameModeStateValue.GameMode != GameModes.Infection)
            {
                MelonLogger.Warning($"Someone in your room is cheating, unable to identify. Reason: {playerID} illegally requested a powerup");
                return false;
            }
        }
        return true;
    }
}