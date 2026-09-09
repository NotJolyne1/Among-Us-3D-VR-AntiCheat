using AntiCheat.Config;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using UnityEngine;
using static Il2CppFusion.Simulation;
using static MelonLoader.MelonLogger;

namespace AntiCheat.Managers.AntiCheat
{
    internal class AntiCheatMain
    {
        private static readonly Dictionary<int, int> MeetingsCalled = new();

        internal static void Update()
        {
            if (!Settings.InGame || GameReferences.Rig == null) return;
            Settings.IsHost = GameReferences.Rig.PState.PlayerId == 9;

            if (Settings.NoCooldown && GameReferences.Rig.PState.ActionCooldownRemaining != 0)
                GameReferences.Killing!.SetMaxCooldown(0);
        }

        internal static void ResetAntiCheat()
        {
            Logger.DebugMsg("Resetting Anti-Cheat");
            MeetingsCalled.Clear();
        }

        internal static void Detected(PlayerState cheater, string reason)
        {
            if (Settings.IsHost)
            {
                Logger.Warning($"Kicking {cheater.NetworkName.Value} for cheating. Reason: {reason}");
                Commands.KickPlayerViaAntiCheat(cheater.PlayerId, reason);
            }
        }



        internal static bool VerifyKill(PlayerState killer, PlayerState victim)
        {
            if (killer == null || victim == null) return false;

            if (!victim.IsAlive || killer == victim)
                return false;

            if (GameReferences.GameState!.GameModeStateValue.GameMode == GameModes.Infection)
                return false;

            if (!GameReferences.GameState!.InTaskState())
                return false;

            GameRole KillerRole = Commands.GetPlayerRole(killer.PlayerId);

            if (!killer.IsAlive && KillerRole != GameRole.Revenger)
                return false;

            if (KillerRole != GameRole.Impostor && KillerRole != GameRole.Revenger && KillerRole != GameRole.Vigilante)
                return false;

            if ((killer.LocomotionPlayer.RigidbodyPosition - victim.LocomotionPlayer.RigidbodyPosition).magnitude > 5f)
                return false;

            if (killer.ActionCooldownRemaining > 0.1f)
                return false;

            return true;
        }







        internal static bool VerifyInfect(PlayerState tagger, PlayerState victim)
        {
            if (tagger == null || victim == null) return false;
            GameRole victimRole = Commands.GetPlayerRole(victim.PlayerId);

            if (GameReferences.GameState!.GameModeStateValue.GameMode != GameModes.Infection)
                return false;

            if (victimRole == GameRole.Infected || tagger == victim)
                return false;

            if (!GameReferences.GameState!.InTaskState())
                return false;

            GameRole TaggerRole = Commands.GetPlayerRole(tagger.PlayerId);


            if (TaggerRole != GameRole.Infected)
                return false;

            if ((tagger.LocomotionPlayer.RigidbodyPosition - victim.LocomotionPlayer.RigidbodyPosition).magnitude > 5f)
                return false;

            if (tagger.ActionCooldownRemaining > 0.1f)
                return false;

            return true;
        }

















        internal static bool VerifyMeeting(PlayerState caller, ref RpcInfo info)
        {
            if (caller == null)
                return false;

            if (info.Source.IsNone && caller.PlayerId == 9)
                info.Source = GameReferences.Runner!.LocalPlayer;

            PlayerState InfoCaller = Helpers.GetPlayerstateFromID(info.Source.PlayerId);

            if (InfoCaller == null)
                return false;

            if (InfoCaller != caller)
                return false;

            MeetingsCalled.TryGetValue(info.Source.PlayerId, out int called);

            if (called >= GameReferences.Button!._maxAllowedCalls)
                return false;

            GameObject button = GameObject.Find("UI_EmergencyButton");
            if (button == null) return false;

            if ((caller.LocomotionPlayer.RigidbodyPosition - button.transform.position).magnitude > 5f)
                return false;

            MeetingsCalled[info.Source.PlayerId] = called + 1;

            if (!GameReferences.GameState!.InTaskState() || GameReferences.GameState.GameModeStateValue.GameMode == GameModes.Infection)
                return false;

            return true;
        }

        internal static bool VerifyBodyReport(PlayerState caller, PlayerState reported, ref RpcInfo info)
        {
            if (caller == null || reported == null)
                return false;

            if (info.Source.IsNone && caller.PlayerId == 9)
                info.Source = GameReferences.Runner!.LocalPlayer;

            PlayerState InfoCaller = Helpers.GetPlayerstateFromID(info.Source.PlayerId);

            if (!reported.IsAlive)
                return false;

            if (!GameReferences.Spawn!._playerIdToBody.TryGetValue(reported.PlayerId, out var BodyObj))
                return false;

            NetworkedBody body = BodyObj.GetComponent<NetworkedBody>();

            if (body == null || !body._playerBody.active)
                return false;

            if ((InfoCaller.LocomotionPlayer.RigidbodyPosition - body._playerBody.transform.position).magnitude > 5f)
                return false;

            if (GameReferences.GameState!.GameModeStateValue.GameMode == GameModes.Infection)
                return false;

            if (!GameReferences.GameState!.InTaskState())
                return false;
            return true;
        }


        internal static bool VerifyBodySpawn(NetworkedBody body)
        {
            if (GameReferences.Spawn == null)
                return false;

            if (body == null)
                return false;

            if (body._playerState == null)
                return false;

            if (body._playerBody == null)
                return false;


            if (body._playerState.IsAlive)
                return false;

            if (GameReferences.GameState == null)
                return false;

            if (GameReferences.GameState.GameModeStateValue.GameMode == GameModes.Infection)
                return false;

            if (!GameReferences.GameState.InTaskState())
                return false;

            return true;
        }

        internal static bool VerifySkipVote(PlayerRef voter, ref RpcInfo info)
        {
            if (info.Source.IsNone)
                info.Source = GameReferences.Runner!.LocalPlayer;

            PlayerState InfoVoter = Helpers.GetPlayerstateFromID(info.Source.PlayerId);

            if (InfoVoter == null)
                return false;

            if (voter != info.Source)
                return false;

            if (GameReferences.GameState == null || !GameReferences.GameState.InVotingState())
                return false;

            if (!InfoVoter.IsAlive)
                return false;

            return true;
        }

        internal static bool VerifyVote(PlayerRef voter, PlayerRef voted, ref RpcInfo info)
        {
            if (info.Source.IsNone)
                info.Source = GameReferences.Runner!.LocalPlayer;

            PlayerState InfoVoter = Helpers.GetPlayerstateFromID(info.Source.PlayerId);
            PlayerState VoterState = Helpers.GetPlayerstateFromID(voter.PlayerId);
            PlayerState VotedState = Helpers.GetPlayerstateFromID(voted.PlayerId);

            if (InfoVoter == null || VoterState == null || VotedState == null)
                return false;

            if (voter != info.Source)
                return false;

            if (GameReferences.GameState == null || !GameReferences.GameState.InVotingState())
                return false;

            if (!InfoVoter.IsAlive || !VotedState.IsAlive)
                return false;

            return true;
        }


        internal static bool VerifyCosmeticChange(NetworkedLocomotionPlayer instance, int cosmetic, int player)
        {
            if (instance.PState.PlayerId != player)
                return false;

            if (cosmetic < 0 || cosmetic > GameReferences.Customization!._elementCollection.AllCustomizationElements.Count)
                return false;

            if (cosmetic == 98)
                return false;

            return true;
        }


        internal static bool VerifyDeputyVote(PlayerState deputy, PlayerState voted)
        {
            if (deputy == null || voted == null) return false;

            if (!voted.IsAlive || !voted.IsSpectating)
                return false;

            if (deputy == voted)
                return false;

            if (!GameReferences.GameState!.InVotingState())
                return false;

            if (GameReferences.Vote!.SheriffId != deputy.PlayerId || Commands.GetPlayerRole(deputy.PlayerId) != GameRole.Sheriff)
                return false;

            return true;
        }


        internal static bool VerifyJoin(NetworkedLocomotionPlayer joining, string UserId, int hat)
        {
            if (hat == 98)
                return false;

            if (joining.Runner.GetPlayerUserId() != UserId)
                return false;

            return true;
        }
    }
}
