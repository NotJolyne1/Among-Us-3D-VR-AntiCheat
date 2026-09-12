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
        if (Settings.IsHost && Settings.AntiCheatEnabled && Settings.SpawnBodyValidateModule)
        {
            if (!AntiCheatMain.VerifyBodySpawn(id, rb))
            {
                if (__instance?.NetworkedBodies != null)
                {
                    foreach (NetworkedBody Body in __instance.NetworkedBodies)
                    {
                        if (Body?._playerState == null)
                            continue;

                        if (Body._playerState.PlayerId == id.PlayerId)
                            Body.RPC_ToggleBody(false);
                    }
                }

                MelonLogger.Warning("Body despawned cheater");
            }
        }
    }
}