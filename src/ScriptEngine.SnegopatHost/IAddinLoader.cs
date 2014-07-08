using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ScriptEngine.SnegopatHost
{
    [ComVisible(true)]
    [Guid("2BEEF9E6-AF34-4593-9E73-3D07EAA4CF0D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAddinLoader
    {
        [return : MarshalAs( UnmanagedType.Interface )] object load(
            [In, MarshalAs(UnmanagedType.BStr)] string uri,
            [Out, MarshalAs(UnmanagedType.BStr)] out string fullPath,
            [Out, MarshalAs(UnmanagedType.BStr)] out string uniqueName,
            [Out, MarshalAs(UnmanagedType.BStr)] out string displayName);            

        [return: MarshalAs(UnmanagedType.Bool)] bool canUnload(
            [In, MarshalAs(UnmanagedType.BStr)] string fullPath,
            [In, MarshalAs(UnmanagedType.Interface)] object addin);

        [return: MarshalAs(UnmanagedType.Bool)] bool unload(
            [In, MarshalAs(UnmanagedType.BStr)] string fullPath,
            [In, MarshalAs(UnmanagedType.Interface)] object addin);

        [return: MarshalAs(UnmanagedType.BStr)] string loadCommandName();
        [return: MarshalAs(UnmanagedType.BStr)] string selectLoadURI();

    }
}
