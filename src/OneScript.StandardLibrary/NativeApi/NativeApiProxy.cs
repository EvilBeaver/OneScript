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
    /// Трансляция вызовов C# для взаимодействия с NativeApi
    /// </summary>
    static class NativeApiProxy
    {
        private const String ProxyDll = "ScriptEngine.NativeApi.dll";

        public delegate void PointerDelegate(IntPtr ptr);

        public delegate void ArrayDelegate(IntPtr ptr, Int32 number);

        public delegate void OnErrorDelegate(UInt16 wcode, IntPtr source, IntPtr descr, Int32 scode);

        public delegate void OnEventDelegate(IntPtr source, IntPtr message, IntPtr data);

        public delegate void OnStatusDelegate(IntPtr status);

        public static String Str(IntPtr p)
        {
            return p != IntPtr.Zero ? Marshal.PtrToStringUni(p) : String.Empty;
        }

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetClassObject(IntPtr module, string name, OnErrorDelegate onError, OnEventDelegate onEvent, OnStatusDelegate onStatus);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void DestroyObject(IntPtr ptr);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Int32 GetNProps(IntPtr ptr);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Int32 FindProp(IntPtr ptr, string wsPropName);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool IsPropReadable(IntPtr ptr, Int32 lPropNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool IsPropWritable(IntPtr ptr, Int32 lPropNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetPropName(IntPtr ptr, Int32 lPropNum, Int32 lPropAlias, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetPropVal(IntPtr ptr, Int32 lPropNum, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void SetPropVal(IntPtr ptr, Int32 lPropNum, ref NativeApiVariant value);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Int32 GetNMethods(IntPtr ptr);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Int32 FindMethod(IntPtr ptr, string wsMethodName);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetMethodName(IntPtr ptr, Int32 lMethodNum, Int32 lMethodAlias, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern Int32 GetNParams(IntPtr ptr, Int32 lMethodNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetParamDefValue(IntPtr ptr, Int32 lMethodNum, Int32 lParamNum, PointerDelegate response);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool HasRetVal(IntPtr ptr, Int32 lMethodNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CallAsProc(IntPtr ptr, Int32 lMethodNum, IntPtr value);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CallAsFunc(IntPtr ptr, Int32 lMethodNum, IntPtr value, PointerDelegate response);
    }
}