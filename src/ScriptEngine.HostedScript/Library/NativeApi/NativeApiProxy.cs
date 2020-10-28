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
    /// <summary>
    /// Трансляция вызовов C# для взаимодействия с NativeApi
    /// </summary>
    static class NativeApiProxy
    {
        private const String ProxyDll = "ScriptEngine.NativeApi.dll";

        public delegate void PointerDelegate(IntPtr ptr);

        public delegate void ArrayDelegate(IntPtr ptr, long number);

        public static String Str(IntPtr p)
        {
            return p != IntPtr.Zero ? Marshal.PtrToStringUni(p) : String.Empty;
        }

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetClassObject(IntPtr module, string name);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long DestroyObject(IntPtr ptr);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetNProps(IntPtr ptr);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long FindProp(IntPtr ptr, string wsPropName);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool IsPropReadable(IntPtr ptr, long lPropNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool IsPropWritable(IntPtr ptr, long lPropNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetPropName(IntPtr ptr, long lPropNum, long lPropAlias, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetPropVal(IntPtr ptr, long lPropNum, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void SetPropVal(IntPtr ptr, long lPropNum, ref NativeApiVariant value);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetNMethods(IntPtr ptr);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long FindMethod(IntPtr ptr, string wsMethodName);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetMethodName(IntPtr ptr, long lMethodNum, long lMethodAlias, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetNParams(IntPtr ptr, long lMethodNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetParamDefValue(IntPtr ptr, long lMethodNum, long lParamNum, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool HasRetVal(IntPtr ptr, long lMethodNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CallAsProc(IntPtr ptr, long lMethodNum, IntPtr value);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CallAsFunc(IntPtr ptr, long lMethodNum, IntPtr value, PointerDelegate response);
    }
}