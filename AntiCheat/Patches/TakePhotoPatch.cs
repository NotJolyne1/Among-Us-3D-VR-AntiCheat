using AntiCheat.Config;
using HarmonyLib;
using Il2CppFusion;
using Il2CppSG.Airlock.Network;
using Il2CppSystem.IO;
using MelonLoader;
using UnityEngine;

[HarmonyPatch(typeof(GlobalRPCCaller), nameof(GlobalRPCCaller.Play), new Type[] { })]
public static class CameraPlayPatch
{
    static readonly List<float> Clicks = new();
    static float BlockedUntil;

    [HarmonyPrefix]
    public static bool Prefix(GlobalRPCCaller __instance)
    {

        if (!Settings.IsHost || !Settings.AntiCheatEnabled) return false;

        var obj = GameObject.Find("InteractionComponents");
        if (obj == null || obj.GetComponent<GlobalRPCCaller>() != __instance) return true;

        float now = Time.realtimeSinceStartup;
        if (now < BlockedUntil) return false;

        if (!GameReferences.GameState!.InLobbyState())
        {
            MelonLogger.Warning("Someone in your lobby is cheating! Reason: TakePhoto called while game started");
        }

        Clicks.RemoveAll(x => now - x > 1f);
        Clicks.Add(now);

        if (Clicks.Count < 5) return true;

        Clicks.Clear();
        BlockedUntil = now + 10f;

        var steam = new List<PlayerRef>();

        foreach (var player in GameReferences.Runner!.ActivePlayers.ToArray())
        {
            if (player != GameReferences.Runner.LocalPlayer &&
                GameReferences.Runner.GetPlayerUserId(player).StartsWith("Steam_"))
                steam.Add(player);
        }

        if (steam.Count <= 4)
        {
            MelonLogger.Warning($"Kicking {steam.Count} Unwhitelisted Steam players because one of them was attempting to crash everyones game");

            foreach (var player in steam)
                GameReferences.Runner.Disconnect(player);
        }

        return false;
    }
}