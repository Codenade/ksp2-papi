using KSP.Logging;
using System.Runtime.CompilerServices;

namespace ksp2_papi
{
    internal static class Logger
    {
        public static void Log(object o)
        {
            GlobalLog.Log(LogFilter.UserMod, "[" + ModInfo.Name + "] " + o);
        }

        public static void Error(object o)
        {
            GlobalLog.Error(LogFilter.UserMod, "[" + ModInfo.Name + "] " + o);
        }

        public static void LogLine(string msg = "Debug message", [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = null)
        {
            Log(msg + " at line " + lineNumber + " inside " + caller);
        }

        public static void ErrorLine(string msg = "Error", [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = null)
        {
            Error(msg + " at line " + lineNumber + " inside " + caller);
        }
    }
}
