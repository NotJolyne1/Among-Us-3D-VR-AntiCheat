using AntiCheat.Config;
using MelonLoader;

namespace AntiCheat.Managers
{
    internal class Logger
    {
        internal static void Msg(string msg) => MelonLogger.Msg(msg);
        internal static void Warning(string warning) => MelonLogger.Warning(warning);
        internal static void Error(string error) => MelonLogger.Error(error);

        internal static void DebugMsg(string msg)
        {
            if (Settings.DebugMode) MelonLogger.Msg(msg);
        }

        internal static void DebugWarning(string warning)
        {
            if (Settings.DebugMode) MelonLogger.Warning(warning);
        }

    }
}
