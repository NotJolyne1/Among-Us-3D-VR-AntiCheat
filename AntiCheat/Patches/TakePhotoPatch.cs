using AntiCheat.Config;
using AntiCheat.Managers;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;

[HarmonyPatch(typeof(GlobalRPCCaller), nameof(GlobalRPCCaller.Play), new Type[] { })]
public static class CameraPlayPatch
{
    internal static readonly List<float> PhotosTaken = new();
    private static float BlockedUntil;

    [HarmonyPrefix]
    public static bool Prefix(GlobalRPCCaller __instance)
    {

        if (!Settings.IsHost || !Settings.AntiCheatEnabled || !Settings.TakePhotoValidateModule) return true;

        var obj = GameObject.Find("InteractionComponents");
        if (obj == null || obj.GetComponent<GlobalRPCCaller>() != __instance) return true;

        float now = Time.realtimeSinceStartup;
        bool blocked = now < BlockedUntil;

        if (blocked) BlockedUntil = now + 1f;

        if (!GameReferences.GameState!.InLobbyState())
        {
            MelonLogger.Warning("Someone in your lobby is cheating! Reason: TakePhoto called while game started");
        }

        PhotosTaken.RemoveAll(x => now - x > 1f);
        PhotosTaken.Add(now);

        if (PhotosTaken.Count < 5) return !blocked;

        BlockedUntil = now + 1f;
        PhotosTaken.Clear();

        var SteamPlayers = new List<PlayerState>();

        if (GameReferences.Spawn == null || GameReferences.Runner == null)
            GameReferences.ResetReferences();

        var runner = GameReferences.Runner;
        var spawn = GameReferences.Spawn;

        if (runner == null || spawn == null)
            return false;

        foreach (PlayerState player in spawn.ActivePlayerStates)
        {
            if (player == null) continue;

            try
            {
                if (!player.IsConnected || player.PlayerId == runner.LocalPlayer.PlayerId) continue;

                string userId = runner.GetPlayerUserId(player.PlayerId);

                if (!string.IsNullOrEmpty(userId) && userId.StartsWith("Steam_") && !player.IsWhitelisted())
                {
                    SteamPlayers.Add(player);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning($"[ANTI-CRASH] Could not check a player: {ex.Message}");
            }
        }

        MelonLogger.Warning($"[ANTI-CRASH] Kicking {SteamPlayers.Count} Unwhitelisted Steam players because one of them was attempting to crash everyones game");
        MelonLogger.Warning($"[ANTI-CRASH] While some of these players may have been innocent, it would have resulted in your game being crashed.");

        foreach (PlayerState player in SteamPlayers)
            GameReferences.Runner!.Disconnect(player.PlayerId);


        return false;
    }
}