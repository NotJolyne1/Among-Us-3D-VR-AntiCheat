using AntiCheat.Config;
using AntiCheat.Managers;
using AntiCheat.Managers.AntiCheat;
using MelonLoader;
using UnityEngine.InputSystem;

[assembly: MelonInfo(typeof(AntiCheat.Main), "Anti-Cheat Mod", "1.0.0", "Jolyne")]
[assembly: MelonGame("Innersloth", null)]
namespace AntiCheat
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
#if DEBUG
            Settings.DebugMode = true;
#else
            Settings.DebugMode = false;
#endif
            Logger.Msg("Anti-Cheat loaded");
        }


        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            Settings.InGame = sceneName != "Boot" && sceneName != "Title";
            AntiCheatMain.ResetAntiCheat();

            Logger.DebugMsg($"Scene {sceneName}");
        }

        public override void OnGUI()
        {
            GUIManager.Display();
        }

        public override void OnUpdate()
        {
            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
                Settings.GUIEnabled = !Settings.GUIEnabled;

            if (Settings.InGame && GameReferences.Rig == null) GameReferences.ResetReferences();

            AntiCheatMain.Update();
        }
    }
}

/*
 * Kill verification
 * Infect verification
 * SheriffVote verification
 * Meeting verification
 * Body Report verification
 * Body Spawn verification
 * vote verification
 * KickVote Verification
 * Crash Verification
 * Cosmetic Change Verification
 * Join Verification
 * Name Change Verification
*/