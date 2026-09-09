using AntiCheat.Config;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Roles;

namespace AntiCheat.Managers
{
    internal class Commands
    {
        internal static void KickPlayerViaAntiCheat(int player, string reason, bool blacklist)
        {
            if (Settings.IsHost)
            {
                GameReferences.Runner?.Disconnect(player);
            }
        }


        internal static GameRole GetPlayerRole(PlayerRef player)
        {
            if (!player.IsValid) return GameRole.NotSet;

            foreach (var pair in GameReferences.Role!.gameRoleToPlayerIds)
                if (pair.Value.Contains(player)) return pair.Key;

            return GameRole.NotSet;
        }
    }
}
