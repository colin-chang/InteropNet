using System;
using System.Diagnostics;
using System.Globalization;

namespace ColinChang.InteropNet
{
    internal static class LibraryLoaderTrace
    {
        private const bool PrintToConsole = false;

        private static void Print(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void TraceInformation(string format, params object[] args)
        {
            Trace.TraceInformation(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        public static void TraceError(string format, params object[] args)
        {
            Trace.TraceError(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        public static void TraceWarning(string format, params object[] args)
        {
            Trace.TraceWarning(string.Format(CultureInfo.CurrentCulture, format, args));
        }
    }
}