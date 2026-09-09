using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;

[HarmonyPatch(typeof(SpawnManager), nameof(SpawnManager.OnBodySpawn))]
public static class SpawnBodyPatch
{
    [HarmonyPostfix]
    public static void Postfix(NetworkedBody body)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyBodySpawn(body))
            {
                body?.RPC_ToggleBody(false);
                MelonLogger.Warning($"Body despawned cheater");
            }
        }
    }
}