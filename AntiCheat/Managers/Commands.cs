using AntiCheat.Config;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace AntiCheat.Managers
{
    internal static class Commands
    {
        private static readonly HashSet<string> WhitelistedPlayers = new HashSet<string>();
        private static readonly HashSet<string> BlacklistedPlayers = new HashSet<string>();
        private static bool DoorCorrectionPending;

        internal static void KickPlayerViaAntiCheat(int player, string reason, bool blacklist)
        {
            if (Settings.IsHost)
            {
                if (blacklist)
                    BlacklistPlayer(Helpers.GetPlayerstateFromID(player), kick: false);
                MelonCoroutines.Start(QueueKick(player));
            }
        }

        private static IEnumerator QueueKick(int cheater)
        {
            yield return new WaitForEndOfFrame();
            if (GameReferences.Moderation == null) GameReferences.ResetReferences();
            GameReferences.Moderation!.RPC_KickPlayer(cheater);
            yield return new WaitForSeconds(0.25f);
            if (((PlayerRef)cheater).IsValid) GameReferences.Runner!.Disconnect(cheater);
        }

        internal static string Hash(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                return Convert.ToHexString(bytes);
            }
        }


        public static bool IsWhitelisted(this PlayerState player)
        {
            if (!Settings.IsHost || player == null || GameReferences.Runner == null)
                return false;

            string userId = GameReferences.Runner.GetPlayerUserId(player.PlayerId);
            return !string.IsNullOrEmpty(userId)
                && WhitelistedPlayers.Contains(Hash(userId));
        }

        public static bool IsBlacklisted(this PlayerState player)
        {
            if (!Settings.IsHost || player == null || GameReferences.Runner == null)
                return false;

            string userId = GameReferences.Runner.GetPlayerUserId(player.PlayerId);
            return !string.IsNullOrEmpty(userId)
                && BlacklistedPlayers.Contains(Hash(userId));
        }

        public static void WhitelistPlayer(PlayerState player)
        {
            if (!Settings.IsHost || player == null || GameReferences.Runner == null)
                return;

            string userId = GameReferences.Runner.GetPlayerUserId(player.PlayerId);
            if (string.IsNullOrEmpty(userId))
                return;

            string hashedId = Hash(userId);
            WhitelistedPlayers.Add(hashedId);
            BlacklistedPlayers.Remove(hashedId);
        }

        public static void UnwhitelistPlayer(PlayerState player)
        {
            if (!Settings.IsHost || player == null || GameReferences.Runner == null)
                return;

            string userId = GameReferences.Runner.GetPlayerUserId(player.PlayerId);
            if (string.IsNullOrEmpty(userId))
                return;

            WhitelistedPlayers.Remove(Hash(userId));
        }

        public static void BlacklistPlayer(PlayerState player, bool kick = true)
        {
            if (!Settings.IsHost || player == null || GameReferences.Runner == null)
                return;

            string userId = GameReferences.Runner.GetPlayerUserId(player.PlayerId);
            if (string.IsNullOrEmpty(userId))
                return;

            string hashedId = Hash(userId);
            BlacklistedPlayers.Add(hashedId);
            WhitelistedPlayers.Remove(hashedId);

            if (kick)
                KickPlayerViaAntiCheat(player.PlayerId, "Player is on blacklist", false);
        }


        internal static GameRole GetPlayerRole(PlayerRef player)
        {
            if (!player.IsValid) return GameRole.NotSet;

            foreach (var pair in GameReferences.Role!.gameRoleToPlayerIds)
                if (pair.Value.Contains(player)) return pair.Key;

            return GameRole.NotSet;
        }

        internal static IEnumerator CorrectLobbyDoors(GameStateManager manager)
        {
            if (DoorCorrectionPending) yield break;
            DoorCorrectionPending = true;

            try
            {
                yield return new WaitForSeconds(0.35f);

                if (manager == null || !Settings.IsHost || !Settings.AntiCheatEnabled)
                    yield break;

                bool close = manager.InLobbyState() || manager.InVotingState();
                manager.RPC_ToggleLobbyDoors(close);
            }
            finally
            {
                DoorCorrectionPending = false;
            }
        }
    }
}
