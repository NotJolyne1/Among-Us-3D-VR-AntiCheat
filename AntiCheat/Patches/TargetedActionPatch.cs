using AntiCheat;
using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;

[HarmonyPatch(typeof(NetworkedKillBehaviour), nameof(NetworkedKillBehaviour.RPC_TargetedAction))]
public static class TargetedActionPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerRef targetedPlayer, PlayerRef perpetrator, int action)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            PlayerState killer = Helpers.GetPlayerstateFromID(perpetrator.PlayerId);
            PlayerState victim = Helpers.GetPlayerstateFromID(targetedPlayer.PlayerId);

            if (action == (int)ProximityTargetedAction.Kill)
                return AntiCheatMain.VerifyKill(killer, victim);

            if (action == (int)ProximityTargetedAction.Infect)
                return AntiCheatMain.VerifyInfect(killer, victim);

            if (action == (int)ProximityTargetedAction.Infect)
                return AntiCheatMain.VerifyDeputyVote(killer, victim);
        }

        return true;
    }
}