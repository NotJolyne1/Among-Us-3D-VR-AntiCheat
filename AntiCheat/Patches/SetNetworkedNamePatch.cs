using AntiCheat.Config;
using AntiCheat.Managers.AntiCheat;
using HarmonyLib;
using Il2CppSG.Airlock;
using Il2CppSG.Airlock.Network;
using MelonLoader;

[HarmonyPatch(typeof(NetworkedLocomotionPlayer), nameof(NetworkedLocomotionPlayer.RPC_SetNetworkName))]
public static class SetNetworkedNamePatch
{
    [HarmonyPostfix]
    public static void Postfix(NetworkedLocomotionPlayer __instance, string name)
    {
        if (Settings.IsHost && Settings.AntiCheatEnabled && Settings.UsernameValidateModule)
        {
            if (!AntiCheatMain.VerifyName(__instance.PState, name))
            {
                AntiCheatMain.Detected(__instance.PState, "Illegal name change");
            }
        }
    }
}