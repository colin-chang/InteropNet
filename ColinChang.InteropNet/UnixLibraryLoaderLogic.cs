using System;
using System.Runtime.InteropServices;

namespace ColinChang.InteropNet
{
    internal class UnixLibraryLoaderLogic : ILibraryLoaderLogic
    {
        public IntPtr LoadLibrary(string fileName)
        {
            var libraryHandle = IntPtr.Zero;

            try
            {
                LibraryLoaderTrace.TraceInformation("Trying to load native library \"{0}\"...", fileName);
                libraryHandle = UnixLoadLibrary(fileName, RtldNow);
                if (libraryHandle != IntPtr.Zero)
                    LibraryLoaderTrace.TraceInformation("Successfully loaded native library \"{0}\", handle = {1}.", fileName, libraryHandle);
                else
                    LibraryLoaderTrace.TraceError("Failed to load native library \"{0}\".\r\nCheck windows event log.", fileName);
            }
            catch (Exception e)
            {
                var lastError = UnixGetLastError();
                LibraryLoaderTrace.TraceError("Failed to load native library \"{0}\".\r\nLast Error:{1}\r\nCheck inner exception and\\or windows event log.\r\nInner Exception: {2}", fileName, lastError, e.ToString());
            }

            return libraryHandle;
        }

        public bool FreeLibrary(IntPtr libraryHandle)
        {
            return UnixFreeLibrary(libraryHandle) != 0;
        }

        public IntPtr GetProcAddress(IntPtr libraryHandle, string functionName)
        {
            UnixGetLastError(); // Clearing previous errors
            LibraryLoaderTrace.TraceInformation("Trying to load native function \"{0}\" from the library with handle {1}...",
                functionName, libraryHandle);
            var functionHandle = UnixGetProcAddress(libraryHandle, functionName);
            var errorPointer = UnixGetLastError();
            if (errorPointer != IntPtr.Zero)
                throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(errorPointer));
            if (functionHandle != IntPtr.Zero && errorPointer == IntPtr.Zero)
                LibraryLoaderTrace.TraceInformation("Successfully loaded native function \"{0}\", function handle = {1}.",
                    functionName, functionHandle);
            else
                LibraryLoaderTrace.TraceError("Failed to load native function \"{0}\", function handle = {1}, error pointer = {2}",
                    functionName, functionHandle, errorPointer);
            return functionHandle;
        }

        public string FixUpLibraryName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
            if (!fileName.EndsWith(".so", StringComparison.OrdinalIgnoreCase))
                fileName += ".so";
            if (!fileName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
                fileName = "lib" + fileName;
            return fileName;
        }

        private const int RtldNow = 2;

        [DllImport("libdl.so.2", EntryPoint = "dlopen")]
        private static extern IntPtr UnixLoadLibrary(string fileName, int flags);

        [DllImport("libdl.so.2", EntryPoint = "dlclose", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int UnixFreeLibrary(IntPtr handle);

        [DllImport("libdl.so.2", EntryPoint = "dlsym", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr UnixGetProcAddress(IntPtr handle, string symbol);

        [DllImport("libdl.so.2", EntryPoint = "dlerror", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr UnixGetLastError();
    }
}