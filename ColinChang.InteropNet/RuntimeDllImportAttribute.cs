﻿using System;
using System.Runtime.InteropServices;

namespace ColinChang.InteropNet
{
    [ComVisible(true)]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class RuntimeDllImportAttribute : Attribute
    {
        public string EntryPoint;

        public CallingConvention CallingConvention;

        public CharSet CharSet;

        public bool SetLastError;        

        public bool BestFitMapping;

        public bool ThrowOnUnmappableChar;

        public string LibraryFileName { get; private set; }

        public RuntimeDllImportAttribute(string libraryFileName)
        {
            LibraryFileName = libraryFileName;
        }
    }
}