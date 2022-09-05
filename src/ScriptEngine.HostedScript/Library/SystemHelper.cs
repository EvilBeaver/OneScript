/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library
{
    public static class SystemHelper
    {
        public static string UnixKernelName()
        {
            return KernelNameInternal();
        }

        [DllImport("libc")]
        private static extern int uname(IntPtr buf);

        private static string KernelNameInternal()
        {
            IntPtr buf = IntPtr.Zero;
            try
            {
                buf = Marshal.AllocHGlobal(8192);
                if (uname(buf) == 0)
                {
                    var osKernelName = Marshal.PtrToStringAnsi(buf);
                    return osKernelName;
                }
            }
            finally
            {
                if (buf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buf);
                }
            }

            return null;
        }
    }
}   