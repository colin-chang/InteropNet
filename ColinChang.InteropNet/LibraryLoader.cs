using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ColinChang.InteropNet
{
    public class LibraryLoader
    {
        private readonly ILibraryLoaderLogic _logic;

        private LibraryLoader(ILibraryLoaderLogic logic) =>
            _logic = logic;


        private readonly object _syncLock = new object();
        private readonly Dictionary<string, IntPtr> _loadedAssemblies = new Dictionary<string, IntPtr>();

        public IntPtr LoadLibrary(string fileName, string platformName = null)
        {
            fileName = FixUpLibraryName(fileName);
            lock (_syncLock)
            {
                if (_loadedAssemblies.ContainsKey(fileName))
                    return _loadedAssemblies[fileName];
                platformName ??= SystemManager.GetPlatformName();
                LibraryLoaderTrace.TraceInformation("Current platform: " + platformName);
                var dllHandle = CheckExecutingAssemblyDomain(fileName, platformName);
                if (dllHandle == IntPtr.Zero)
                    dllHandle = CheckCurrentAppDomain(fileName, platformName);
                if (dllHandle == IntPtr.Zero)
                    dllHandle = CheckWorkingDirectory(fileName, platformName);

                if (dllHandle != IntPtr.Zero)
                    _loadedAssemblies[fileName] = dllHandle;
                else
                    throw new DllNotFoundException(
                        $"Failed to find library \"{fileName}\" for platform {platformName}.");
                return _loadedAssemblies[fileName];
            }
        }

        private IntPtr CheckExecutingAssemblyDomain(string fileName, string platformName)
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                LibraryLoaderTrace.TraceInformation("Executing assembly location was empty");
                return IntPtr.Zero;
            }

            var baseDirectory = Path.GetDirectoryName(assemblyLocation);
            return InternalLoadLibrary(baseDirectory, platformName, fileName);
        }

        private IntPtr CheckCurrentAppDomain(string fileName, string platformName)
        {
            var appBase = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(appBase))
            {
                LibraryLoaderTrace.TraceInformation("App domain current domain base was empty");
                return IntPtr.Zero;
            }

            var baseDirectory = Path.GetFullPath(appBase);
            return InternalLoadLibrary(baseDirectory, platformName, fileName);
        }

        private IntPtr CheckWorkingDirectory(string fileName, string platformName)
        {
            var currentDirectory = Environment.CurrentDirectory;
            if (string.IsNullOrEmpty(currentDirectory))
            {
                LibraryLoaderTrace.TraceInformation("Current directory was empty");
                return IntPtr.Zero;
            }

            var baseDirectory = Path.GetFullPath(currentDirectory);
            return InternalLoadLibrary(baseDirectory, platformName, fileName);
        }

        private IntPtr InternalLoadLibrary(string baseDirectory, string platformName, string fileName)
        {
            var fullPath = Path.Combine(baseDirectory, Path.Combine(platformName, fileName));
            return File.Exists(fullPath) ? _logic.LoadLibrary(fullPath) : IntPtr.Zero;
        }

        public bool FreeLibrary(string fileName)
        {
            fileName = FixUpLibraryName(fileName);
            lock (_syncLock)
            {
                if (!IsLibraryLoaded(fileName))
                {
                    LibraryLoaderTrace.TraceWarning("Failed to free library \"{0}\" because it is not loaded",
                        fileName);
                    return false;
                }

                if (!_logic.FreeLibrary(_loadedAssemblies[fileName])) return false;
                _loadedAssemblies.Remove(fileName);
                return true;
            }
        }

        public IntPtr GetProcAddress(IntPtr dllHandle, string name)
        {
            return _logic.GetProcAddress(dllHandle, name);
        }

        public bool IsLibraryLoaded(string fileName)
        {
            fileName = FixUpLibraryName(fileName);
            lock (_syncLock)
                return _loadedAssemblies.ContainsKey(fileName);
        }

        private string FixUpLibraryName(string fileName) =>
            _logic.FixUpLibraryName(fileName);

        #region Singleton

        private static LibraryLoader _instance;

        public static LibraryLoader Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var operatingSystem = SystemManager.GetOperatingSystem();
                switch (operatingSystem)
                {
                    case OperatingSystem.Windows:
                        LibraryLoaderTrace.TraceInformation("Current OS: Windows");
                        _instance = new LibraryLoader(new WindowsLibraryLoaderLogic());
                        break;
                    case OperatingSystem.Unix:
                        LibraryLoaderTrace.TraceInformation("Current OS: Unix");
                        _instance = new LibraryLoader(new UnixLibraryLoaderLogic());
                        break;
                    case OperatingSystem.MacOSX:
                        throw new Exception("Unsupported operation system");
                    case OperatingSystem.Unknown:
                        break;
                    default:
                        throw new Exception("Unsupported operation system");
                }

                return _instance;
            }
        }

        #endregion
    }
}