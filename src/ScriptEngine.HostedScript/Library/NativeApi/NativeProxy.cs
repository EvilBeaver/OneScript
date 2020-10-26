using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    static class NativeApiProxy
    {
        private const String ProxyLibrary = "ScriptEngine.NativeApi.dll";
        public delegate void StringDelegate(IntPtr ptr);

        public static String Str(IntPtr p)
        {
            if (p == IntPtr.Zero) return "";
            return Marshal.PtrToStringUni(p);
        }

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetClassNames(string lib);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GetClassObject(string lib, string name);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long DestroyObject(IntPtr ptr);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetNProps(IntPtr ptr);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long FindProp(IntPtr ptr, string wsPropName);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool IsPropReadable(IntPtr ptr, long lPropNum);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool IsPropWritable(IntPtr ptr, long lPropNum);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetPropName(IntPtr ptr, long lPropNum, long lPropAlias, StringDelegate response);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetNMethods(IntPtr ptr);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long FindMethod(IntPtr ptr, string wsMethodName);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void GetMethodName(IntPtr ptr, long lMethodNum, long lMethodAlias, StringDelegate response);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern long GetNParams(IntPtr ptr, long lMethodNum);

        [DllImport(ProxyLibrary, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool HasRetVal(IntPtr ptr, long lMethodNum);
    }
}