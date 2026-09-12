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
    public static bool Prefix()
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled && Settings.TaskCompletedValidateModule)
        {
            if (!GameReferences.GameState!.InLobbyState() && !GameReferences.Cutscene!._isEndGame)
            {
                MelonLogger.Warning("Someone in your lobby is cheating! Reason: Illegal call of ResetTaskProgress");
                return false;
            }
        }
        return true;
    }
}