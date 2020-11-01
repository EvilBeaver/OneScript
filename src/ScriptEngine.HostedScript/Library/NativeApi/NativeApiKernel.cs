﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    /// <summary>
    /// Подключение DLL-файлов библиотеки внешних компонент Native API
    /// </summary>
    class NativeApiKernel
    {
        private const String KernelDll = "kernel32.dll";

        [DllImport(KernelDll, SetLastError = true, CharSet = CharSet.Unicode)]
        protected static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

        [DllImport(KernelDll, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Ansi)]
        protected static extern IntPtr GetProcAddress(IntPtr _module, string procName);

        [DllImport(KernelDll, SetLastError = true, CharSet = CharSet.Unicode)]
        protected static extern bool FreeLibrary(IntPtr _module);

        private const String libdl = "libdl.so";

        [DllImport(libdl)]
        protected static extern IntPtr dlopen(string filename, int flags);

        [DllImport(libdl)]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(libdl)]
        protected static extern IntPtr dlclose(IntPtr handle);
    }
}
