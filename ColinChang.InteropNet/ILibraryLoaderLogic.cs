using System;

namespace ColinChang.InteropNet
{
    internal interface ILibraryLoaderLogic
    {
        IntPtr LoadLibrary(string fileName);
        bool FreeLibrary(IntPtr libraryHandle);
        IntPtr GetProcAddress(IntPtr libraryHandle, string functionName);
        string FixUpLibraryName(string fileName);
    }
}