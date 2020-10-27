using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    static class NativeApiProxy
    {
        private const String ProxyDll = "ScriptEngine.NativeApi.dll";

        public delegate void PointerDelegate(IntPtr ptr);

        public static String Str(IntPtr p)
        {
            if (p == IntPtr.Zero) return "";
            return Marshal.PtrToStringUni(p);
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
        public static extern bool HasRetVal(IntPtr ptr, long lMethodNum);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CallAsProc(IntPtr ptr, long lMethodNum, ref NativeApiVariant paParams, long lSizeArray);

        [DllImport(ProxyDll, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CallAsFunc(IntPtr ptr, long lMethodNum, ref NativeApiVariant paParams, long lSizeArray, PointerDelegate response);
    }
}