using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Minigames;
using MelonLoader;

[HarmonyPatch(typeof(MinigameManager), nameof(MinigameManager.RPC_ResetTaskProgress))]
public static class ResetTaskProgressPatch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        MelonLogger.Warning($"TASK RESET");
    }
}