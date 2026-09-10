using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using System.Collections.Generic;
using UnityEngine;

[HarmonyPatch(typeof(MinigamePlayer), nameof(MinigamePlayer.RPC_IncrementCompletedTasks))]
public static class CompleteTaskPatch
{
    internal static readonly Dictionary<int, Queue<float>> Tasks = new();

    [HarmonyPrefix]
    public static bool Prefix(MinigamePlayer __instance)
    {
        if (!Settings.IsHost || !Settings.AntiCheatEnabled)
            return true;

        if (!AntiCheatMain.VerifyTaskComplete(__instance.PState))
        {
            AntiCheatMain.Detected(__instance.PState, "Illegal task completion");
            return false;
        }

        float Now = Time.realtimeSinceStartup;
        if (!Tasks.TryGetValue(__instance.PState.PlayerId, out Queue<float> Times))
            Tasks[__instance.PState.PlayerId] = Times = new();
        Times.Enqueue(Now);

        while (Times.Count > 0 && Now - Times.Peek() > 3f)
            Times.Dequeue();

        if (Times.Count >= 5)
        {
            AntiCheatMain.Detected(__instance.PState, "Illegal task completion");
            Times.Clear();
        }

        return true;
    }
}