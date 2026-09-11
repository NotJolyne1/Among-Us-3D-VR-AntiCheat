using AntiCheat;
using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using System.Runtime.InteropServices;

[HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_Vote), new Type[] { typeof(PlayerRef), typeof(RpcInfo) })]
public static class SkipVotePatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerRef sourcePlayer, [Optional] ref RpcInfo info)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifySkipVote(sourcePlayer, ref info))
            {
                PlayerState InfoVoter = Helpers.GetPlayerstateFromID(info.Source.PlayerId);

                if (InfoVoter != null)
                    AntiCheatMain.Detected(InfoVoter, "Illegal vote data");
                return false;
            }
        }
        return true;
    }
}

[HarmonyPatch(typeof(VoteManager), nameof(VoteManager.RPC_Vote), new Type[] { typeof(PlayerRef), typeof(PlayerRef), typeof(RpcInfo) })]
public static class VotePatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerRef voteAgainstPlayer, PlayerRef sourcePlayer, [Optional] ref RpcInfo info)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled)
        {
            if (!AntiCheatMain.VerifyVote(sourcePlayer, voteAgainstPlayer, ref info))
            {
                PlayerState InfoVoter = Helpers.GetPlayerstateFromID(info.Source.PlayerId);

                if (InfoVoter != null)
                    AntiCheatMain.Detected(InfoVoter, "Illegal vote data");
                return false;
            }
        }
        return true;
    }
}