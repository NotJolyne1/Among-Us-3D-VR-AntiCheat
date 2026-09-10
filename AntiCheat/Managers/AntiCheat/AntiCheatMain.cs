using AntiCheat.Config;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSG.Airlock.Roles;
using MelonLoader;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AntiCheat.Managers.AntiCheat
{
    internal class AntiCheatMain
    {
        private static readonly Dictionary<int, int> MeetingsCalled = new();
        private static float _checkTimer = 0f;
        private static Dictionary<PlayerState, int> FailedSpeedAttempts = new Dictionary<PlayerState, int>();
        private static Dictionary<PlayerState, Vector3> PrevPosition = new Dictionary<PlayerState, Vector3>();
        private static Dictionary<PlayerState, int> SpeedDetections = new Dictionary<PlayerState, int>();
        private static Dictionary<PlayerState, int> PositionViolations = new Dictionary<PlayerState, int>();
        private static float SpeedTimer;

        internal static void Update()
        {
            if (!Settings.InGame || GameReferences.Rig == null)
            {
                _checkTimer = 0f;
                SpeedTimer = 0f;
                PrevPosition.Clear();
                SpeedDetections.Clear();
                return;
            }

            Settings.IsHost = GameReferences.Rig.PState.PlayerId == 9;
            CheckSpeed();

            if (Settings.NoCooldown && GameReferences.Rig.PState.ActionCooldownRemaining != 0)
                GameReferences.Killing!.SetMaxCooldown(0);

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= 10f)
            {
                _checkTimer = 0f;

                try
                {
                    CheckPlayers();
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"Error occurred during CheckPlayers: {ex}");
                }
            }
        }





        private static void CheckPlayers()
        {
            foreach (PlayerState player in GameReferences.Spawn!.ActivePlayerStates)
            {
                if (player == null || !player.IsConnected || !player.IsSpawned || string.IsNullOrEmpty(player.PlayerModerationID.Value)) continue;

                if (player.IsBlacklisted() && !player.IsWhitelisted()) Detected(player, "Player is on the blacklist");
                if (player.HatId == 98) Detected(player, "Illegal cosmetics");
                if (!VerifyName(player, player.NetworkName.Value)) Detected(player, "Illegal name");
            }
        }




        private static void CheckSpeed()
        {
            SpeedTimer += Time.deltaTime;

            if (SpeedTimer < 1f || GameReferences.Spawn == null)
                return;

            SpeedTimer = 0f;

            foreach (PlayerState Player in GameReferences.Spawn.ActivePlayerStates)
            {
                if (Player == null || !Player.IsConnected || !Player.IsSpawned)
                    continue;

                Vector3 Position = Player.LocomotionPlayer.NetworkRigidbody.Rigidbody.position;

                if (PrevPosition.TryGetValue(Player, out Vector3 LastPosition))
                {
                    float Speed = Vector3.Distance(Position, LastPosition);

                    if (Speed > 9.5f)
                    {
                        SpeedDetections.TryGetValue(Player, out int Detections);

                        if (++Detections >= 4)
                        {
                            Detected(Player, "Speed Hacks Detected");
                            Detections = 0;
                        }

                        SpeedDetections[Player] = Detections;
                    }
                    else
                        SpeedDetections[Player] = 0;
                }

                PrevPosition[Player] = Position;
            }
        }




        internal static void ResetAntiCheat()
        {
            Logger.DebugMsg("Resetting Anti-Cheat");
            MeetingsCalled.Clear();
            PositionViolations.Clear();
            SpeedDetections.Clear();
            PrevPosition.Clear();
            FailedSpeedAttempts.Clear();
            CompleteTaskPatch.Tasks.Clear();
            SpeedTimer = 0f;
        }

        internal static void Detected(PlayerState cheater, string reason)
        {
            if (Settings.IsHost && !cheater.IsWhitelisted())
            {
                Logger.Warning($"Kicking {cheater.NetworkName.Value} for cheating. Reason: {reason}");
                if (Settings.KickCheaters) Commands.KickPlayerViaAntiCheat(cheater.PlayerId, reason, true);
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

            if ((killer.LocomotionPlayer.RigidbodyPosition - victim.LocomotionPlayer.RigidbodyPosition).sqrMagnitude > 5f)
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

            if ((tagger.LocomotionPlayer.RigidbodyPosition - victim.LocomotionPlayer.RigidbodyPosition).sqrMagnitude > 5f)
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


            if ((caller.LocomotionPlayer.RigidbodyPosition - GameReferences.Button._buttonCollider.transform.position).sqrMagnitude > 5f)
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

            if (InfoCaller == null || InfoCaller != caller)
                return false;

            if (reported.IsAlive)
                return false;

            if (!GameReferences.Spawn!._playerIdToBody.TryGetValue(reported.PlayerId, out var BodyObj))
                return false;

            NetworkedBody body = BodyObj.GetComponent<NetworkedBody>();

            if (body == null || !body._playerBody.active)
                return false;

            if ((caller.LocomotionPlayer.RigidbodyPosition - body._playerBody.transform.position).sqrMagnitude > 5f)
                return false;

            if (GameReferences.GameState!.GameModeStateValue.GameMode == GameModes.Infection)
                return false;

            if (!GameReferences.GameState.InTaskState())
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

            int count = GameReferences.Customization!._elementCollection.AllCustomizationElements.Count;

            if (cosmetic < 0 || cosmetic >= count)
                return false;

            if (cosmetic == 98)
                return false;
            return true;
        }

        internal static bool VerifyKickVote(int source, int voted)
        {
            if (source < 0 || source > 9 || voted < 0 || voted > 9 || source == voted)
                return false;

            PlayerState Source = Helpers.GetPlayerstateFromID(source);
            PlayerState Voted = Helpers.GetPlayerstateFromID(voted);

            if (Source == null || Voted == null || Source.IsSpectating || Voted.IsSpectating)
                return false;

            return true;
        }

        internal static bool VerifyDeputyVote(PlayerState deputy, PlayerState voted)
        {
            if (deputy == null || voted == null) return false;

            if (!voted.IsAlive)
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

            if (joining.Runner.GetPlayerUserId(joining.PState.PlayerId) != UserId)
                return false;

            return true;
        }

        internal static bool VerifyName(PlayerState player, string name)
        {
            if (player == null || string.IsNullOrEmpty(name))
                return false;

            if (name.Length > 17)
                return false;

            if (Regex.IsMatch(name, @"[@$%\^&()<>+=]"))
                return false;
            return true;
        }

        internal static bool VerifyVentEnter(NetworkedLocomotionPlayer player)
        {
            if (player == null)
                return false;

            GameRole VenterRole = Commands.GetPlayerRole(player.PState.PlayerId);

            if (VenterRole != GameRole.Impostor && VenterRole != GameRole.Engineer && player.PState.ActivePowerUps == PowerUps.CanVent)
                return false;

            return true;
        }

        internal static bool VerifyVentExit(NetworkedLocomotionPlayer player)
        {
            GameRole VenterRole = Commands.GetPlayerRole(player.PState.PlayerId);

            if (player == null) return false;

            if (VenterRole != GameRole.Impostor && VenterRole != GameRole.Engineer && player.PState.ActivePowerUps == PowerUps.CanVent)
                return false;
            return true;
        }

        internal static bool VerifyUsePowerup(PlayerState player)
        {
            GameRole Role = Commands.GetPlayerRole(player.PlayerId);

            if (player == null) return false;

            if (!GameReferences.GameState!.InTaskState() || GameReferences.GameState!.GameModeStateValue.GameMode != GameModes.Infection || Role == GameRole.Infected || player.ActivePowerUps == PowerUps.None)
                return false;
            return true;
        }

        internal static bool VerifyTaskComplete(PlayerState player)
        {
            if (player == null)
                return false;

            GameRole Role = Commands.GetPlayerRole(player.PlayerId);

            if (player.IsSpectating || !GameReferences.GameState!.InTaskState())
                return false;

            if (Role == GameRole.Impostor || Role == GameRole.Revenger || Role == GameRole.Infected)
                return false;

            return true;
        }
    }
}
