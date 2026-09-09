using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;

[HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_SpawnInitialization))]
public static class SpawnInitializationPatch
{
    [HarmonyPostfix]
    public static void Postfix(NetworkedLocomotionPlayer __instance, int color, int hat, int hands, int skin, string name, string moderationID, string moderationUsername, string accountID, bool is3D)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyJoin(__instance, moderationID, hat))
            {
                AntiCheatMain.Detected(__instance.PState, "Invalid join data");
            }
        }
    }
}