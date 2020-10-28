/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    class NativeApiKernel
    {
        protected const String KernelDll = "kernel32.dll";

        [DllImport(KernelDll, SetLastError = true, CharSet = CharSet.Unicode)]
        protected static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

        [DllImport(KernelDll, SetLastError = true, ExactSpelling = true, CharSet = CharSet.Ansi)]
        protected static extern IntPtr GetProcAddress(IntPtr _module, string procName);

        [DllImport(KernelDll, SetLastError = true, CharSet = CharSet.Unicode)]
        protected static extern bool FreeLibrary(IntPtr _module);
    }
}
