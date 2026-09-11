using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;

[HarmonyPatch(typeof(SpawnManager), nameof(SpawnManager.RPC_SpawnBodyByPlayerId))]
public static class SpawnBodyPatch
{
    [HarmonyPostfix]
    public static void Postfix(SpawnManager __instance, PlayerRef id, NetworkRigidbody rb)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyBodySpawn(id, rb))
            {
                foreach (NetworkedBody body in __instance.NetworkedBodies)
                {
                    if (body._playerState.LocomotionPlayer.NetworkRigidbody == rb || body._playerState.PlayerId == id.PlayerId)
                        body?.RPC_ToggleBody(false);
                }
                MelonLogger.Warning($"Body despawned cheater");
            }
        }
    }
}