using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;

[HarmonyPatch(typeof(GameStateManager), nameof(GameStateManager.EndAnimation))]
public static class GameEndPatch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        AntiCheatMain.ResetAntiCheat();
    }
}