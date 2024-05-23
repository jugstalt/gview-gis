using System;
using System.Runtime.InteropServices;

namespace gView.Framework.Common
{
    public static class Wow
    {
        public static bool Is64BitProcess
        {
            get { return nint.Size == 8; }
        }

        public static bool Is64BitOperatingSystem
        {
            get
            {
                // Clearly if this is a 64-bit process we must be on a 64-bit OS.
                if (Is64BitProcess)
                {
                    return true;
                }
                // Ok, so we are a 32-bit process, but is the OS 64-bit?
                // If we are running under Wow64 than the OS is 64-bit.
                bool isWow64;
                return ModuleContainsFunction("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out isWow64) && isWow64;
            }
        }

        static bool ModuleContainsFunction(string moduleName, string methodName)
        {
            nint hModule = GetModuleHandle(moduleName);
            if (hModule != nint.Zero)
            {
                return GetProcAddress(hModule, methodName) != nint.Zero;
            }

            return false;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        extern static bool IsWow64Process(nint hProcess, [MarshalAs(UnmanagedType.Bool)] out bool isWow64);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        extern static nint GetCurrentProcess();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        extern static nint GetModuleHandle(string moduleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        extern static nint GetProcAddress(nint hModule, string methodName);
    }
}
