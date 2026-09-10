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

        internal static void KickPlayerViaAntiCheat(int player, string reason, bool blacklist)
        {
            if (Settings.IsHost)
            {
                if (blacklist) BlacklistPlayer(Helpers.GetPlayerstateFromID(player));
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
            if (Settings.IsHost && player != null)
            {
                string UserId = Hash(GameReferences.Runner!.GetPlayerUserId(player.PlayerId));
                string HashedId = Hash(UserId);
                return WhitelistedPlayers.Contains(HashedId);
            }
            return false;
        }

        public static bool IsBlacklisted(this PlayerState player)
        {
            if (Settings.IsHost && player != null)
            {
                string UserId = Hash(GameReferences.Runner!.GetPlayerUserId(player.PlayerId));
                string HashedId = Hash(UserId);
                return BlacklistedPlayers.Contains(HashedId);
            }
            return false;
        }


        public static void WhitelistPlayer(PlayerState player)
        {
            if (Settings.IsHost && player != null)
            {
                string UserId = Hash(GameReferences.Runner!.GetPlayerUserId(player.PlayerId));
                string HashedId = Hash(UserId);

                if (!string.IsNullOrEmpty(HashedId))
                {
                    WhitelistedPlayers.Add(HashedId);
                    BlacklistedPlayers.Remove(HashedId);
                }
            }
        }

        public static void UnwhitelistPlayer(PlayerState player)
        {
            if (Settings.IsHost && player != null && GameReferences.Runner != null)
            {
                string UserId = GameReferences.Runner.GetPlayerUserId(player.PlayerId);
                string HashedId = Hash(UserId);

                if (!string.IsNullOrEmpty(HashedId))
                {
                    WhitelistedPlayers.Remove(HashedId);
                }
            }
        }


        public static void BlacklistPlayer(PlayerState player)
        {
            if (Settings.IsHost && player != null)
            {
                string UserId = Hash(GameReferences.Runner!.GetPlayerUserId(player.PlayerId));
                string HashedId = Hash(UserId);

                if (!string.IsNullOrEmpty(HashedId))
                {
                    BlacklistedPlayers.Add(HashedId);
                    WhitelistedPlayers.Remove(HashedId);

                    KickPlayerViaAntiCheat(player.PlayerId, "Player is on blacklist", false);
                }
            }
        }


        internal static GameRole GetPlayerRole(PlayerRef player)
        {
            if (!player.IsValid) return GameRole.NotSet;

            foreach (var pair in GameReferences.Role!.gameRoleToPlayerIds)
                if (pair.Value.Contains(player)) return pair.Key;

            return GameRole.NotSet;
        }

        internal static IEnumerator CorrectLobbyDoors(GameStateManager manager, bool status)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.35f);
            manager.RPC_ToggleLobbyDoors(!status);
        }
    }
}
