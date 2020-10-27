using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
