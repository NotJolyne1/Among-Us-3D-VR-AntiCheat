using AntiCheat;
using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock.Network;
using MelonLoader;
using UnityEngine;

[HarmonyPatch(typeof(ModerationManager), nameof(ModerationManager.RPC_KickVote))]
public static class KickVotePatch
{
    internal static readonly Dictionary<int, List<(int Source, float Time)>> RecentKickVotes = new();

    [HarmonyPrefix]
    public static bool Prefix(ModerationManager __instance, int sourcePlayer, int kickPlayer)
    {
        if (!Settings.IsHost || !Settings.AntiCheatEnabled || !Settings.KickVoteValidateModule) return true;

        if (!AntiCheatMain.VerifyKickVote(sourcePlayer, kickPlayer))
        {
            MelonLogger.Warning($"Someone in your room is cheating, reason: Force Kick detected");
            return false;
        }

        float now = Time.realtimeSinceStartup;
        if (!RecentKickVotes.TryGetValue(kickPlayer, out var recent)) RecentKickVotes[kickPlayer] = recent = new();

        recent.RemoveAll(x => now - x.Time > .55f);
        recent.Add((sourcePlayer, now));

        if (recent.Count >= 3 && GameReferences.Spawn != null)
        {
            var players = GameReferences.Spawn.ActivePlayerStates;
            for (int i = 0; i <= players.Count - 3; i++)
            {
                if (players[i] == null || players[i + 1] == null || players[i + 2] == null) continue;
                if (recent[recent.Count - 3].Source == players[i].PlayerId && recent[recent.Count - 2].Source == players[i + 1].PlayerId && recent[recent.Count - 1].Source == players[i + 2].PlayerId)
                {
                    recent.Clear();
                    return false;
                }
            }
        }
        return true;
    }
}
