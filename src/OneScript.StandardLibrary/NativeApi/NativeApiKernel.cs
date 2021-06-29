/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Подключение DLL-файлов библиотеки внешних компонент Native API
    /// </summary>
    public class NativeApiKernel
    {
        private const string KernelDll = "kernel32.dll";
        public static bool IsLinux
        {
            get => System.Environment.OSVersion.Platform == PlatformID.Unix;
        }

        private const String KernelWin = "kernel32.dll";
        private const String KernelLin = "libdl.so";

        public static IntPtr LoadLibrary(string filename)
        {
            return IsLinux ? LinuxLoad(filename, 1) : WindowsLoad(filename);
        }

        public static IntPtr GetProcAddress(IntPtr module, string procName)
        {
            return IsLinux ? LinuxProc(module, procName) : WindowsProc(module, procName);
        }

        public static bool FreeLibrary(IntPtr module)
        {
            return IsLinux ? LinuxFree(module) == 0 : WindowsFree(module);
        }

        [DllImport(KernelWin, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LoadLibrary")]
        protected static extern IntPtr WindowsLoad(string lpLibFileName);

        [DllImport(KernelWin, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress")]
        protected static extern IntPtr WindowsProc(IntPtr module, string procName);

        [DllImport(KernelWin, SetLastError = true, EntryPoint = "FreeLibrary")]
        protected static extern bool WindowsFree(IntPtr module);

        [DllImport(KernelLin, EntryPoint = "dlopen")]
        protected static extern IntPtr LinuxLoad(string filename, int flags);

        [DllImport(KernelLin, EntryPoint = "dlsym")]
        protected static extern IntPtr LinuxProc(IntPtr handle, string symbol);

        [DllImport(KernelLin, EntryPoint = "dlclose")]
        protected static extern int LinuxFree(IntPtr handle);
    }
}
