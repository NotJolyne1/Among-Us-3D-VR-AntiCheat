using AntiCheat;
using AntiCheat.Config;
using AntiCheat.Managers;
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

            if (action == (int)ProximityTargetedAction.Vote)
                return AntiCheatMain.VerifyDeputyVote(killer, victim);


            if (action == (int)ProximityTargetedAction.KillSelf)
            {
                if (killer == null) return false;

                if (Commands.GetPlayerRole(killer.PlayerId) != GameRole.Revenger)
                    return false;

                if (!GameReferences.GameState!.InTaskState())
                    return false;
            }

            if (action == (int)ProximityTargetedAction.Neutralize)
                return AntiCheatMain.VerifyPowerup(killer, victim, PowerUps.Stun);

            if (action == (int)ProximityTargetedAction.Guard)
                return AntiCheatMain.VerifyPowerup(killer, victim, PowerUps.Guard);

        }

        return true;
    }
}