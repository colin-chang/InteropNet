using System;

namespace ColinChang.InteropNet
{
    internal static class SystemManager
    {
        public static string GetPlatformName()
        {
            return IntPtr.Size == sizeof(int) ? "x86" : "x64";
        }

        public static OperatingSystem GetOperatingSystem()
        {
            var pid = (int)Environment.OSVersion.Platform;
            switch (pid)
            {
                case (int)PlatformID.Win32NT:
                case (int)PlatformID.Win32S:
                case (int)PlatformID.Win32Windows:
                case (int)PlatformID.WinCE:
                    return OperatingSystem.Windows;
                case (int)PlatformID.Unix:
                case 128:
                    return OperatingSystem.Unix;
                case (int)PlatformID.MacOSX:
                    return OperatingSystem.MacOSX;
                default:
                    return OperatingSystem.Unknown;
            }
        }
    }

    internal enum OperatingSystem
    {
        Windows,
        Unix,
        MacOSX,
        Unknown
    }
}