using AntiCheat.Managers;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Customization;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using Il2CppSG.Airlock.UI;
using Il2CppSG.Airlock.XR;
using MelonLoader;
using static UnityEngine.Object;

namespace AntiCheat.Config
{
    internal class GameReferences
    {
        internal static NetworkRunner? Runner;
        internal static SpawnManager? Spawn;
        internal static GameStateManager? GameState;
        internal static RoleManager? Role;
        internal static XRRig? Rig;
        internal static NetworkedKillBehaviour? Killing;
        internal static UILobbyScreenUI? Lobby;
        internal static EmergencyButton? Button;
        internal static VoteManager? Vote;
        internal static CustomizationManager? Customization;
        internal static ModerationManager? Moderation;


        public static void ResetReferences()
        {
            Runner = null;
            Spawn = null;
            GameState = null;
            Role = null;
            Rig = null;
            Killing = null;
            Lobby = null;
            Button = null;
            Vote = null;
            Customization = null;
            Moderation = null;

            Runner = FindObjectOfType<NetworkRunner>();
            Spawn = FindObjectOfType<SpawnManager>();
            GameState = FindObjectOfType<GameStateManager>();
            Role = FindObjectOfType<RoleManager>();
            Rig = FindObjectOfType<XRRig>();
            Killing = FindObjectOfType<NetworkedKillBehaviour>();
            Lobby = FindObjectOfType<UILobbyScreenUI>();
            Button = FindObjectOfType<EmergencyButton>();
            Vote = FindObjectOfType<VoteManager>();
            Customization = FindObjectOfType<CustomizationManager>();
            Moderation = FindObjectOfType<ModerationManager>();
            Logger.DebugMsg("Game references reset");
        }
    }
}
