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
    class NativeApiProxy
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr TGetClassObject(IntPtr module, [MarshalAs(UnmanagedType.LPWStr)] string name, OnErrorDelegate onError, OnEventDelegate onEvent, OnStatusDelegate onStatus);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TDestroyObject(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TGetNProps(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TFindProp(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string wsPropName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TIsPropReadable(IntPtr ptr, Int32 lPropNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TIsPropWritable(IntPtr ptr, Int32 lPropNum);

        public delegate void TGetPropName(IntPtr ptr, Int32 lPropNum, Int32 lPropAlias, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TGetPropVal(IntPtr ptr, Int32 lPropNum, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetPropVal(IntPtr ptr, Int32 lPropNum, ref NativeApiVariant value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TGetNMethods(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TFindMethod(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string wsMethodName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TGetMethodName(IntPtr ptr, Int32 lMethodNum, Int32 lMethodAlias, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TGetNParams(IntPtr ptr, Int32 lMethodNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TGetParamDefValue(IntPtr ptr, Int32 lMethodNum, Int32 lParamNum, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool THasRetVal(IntPtr ptr, Int32 lMethodNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TCallAsProc(IntPtr ptr, Int32 lMethodNum, IntPtr value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TCallAsFunc(IntPtr ptr, Int32 lMethodNum, IntPtr value, PointerDelegate response);

        public TGetClassObject GetClassObject;
        public TDestroyObject DestroyObject;
        public TGetNProps GetNProps;
        public TFindProp FindProp;
        public TIsPropReadable IsPropReadable;
        public TIsPropWritable IsPropWritable;
        public TGetPropName GetPropName;
        public TGetPropVal GetPropVal;
        public TSetPropVal SetPropVal;
        public TGetNMethods GetNMethods;
        public TFindMethod FindMethod;
        public TGetMethodName GetMethodName;
        public TGetNParams GetNParams;
        public TGetParamDefValue GetParamDefValue;
        public THasRetVal HasRetVal;
        public TCallAsProc CallAsProc;
        public TCallAsFunc CallAsFunc;

        public NativeApiProxy()
        {
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string filename = System.IO.Path.GetDirectoryName(location)
                + System.IO.Path.DirectorySeparatorChar + "ScriptEngine.NativeApi"
                + (NativeApiKernel.IsLinux ? (IntPtr.Size == 8 ? "64" : "32") + ".so" : ".dll");
            IntPtr module = NativeApiKernel.LoadLibrary(filename);
            GetClassObject = Marshal.GetDelegateForFunctionPointer<TGetClassObject>(NativeApiKernel.GetProcAddress(module, "GetClassObject"));
            DestroyObject = Marshal.GetDelegateForFunctionPointer<TDestroyObject>(NativeApiKernel.GetProcAddress(module, "DestroyObject"));
            GetNProps = Marshal.GetDelegateForFunctionPointer<TGetNProps>(NativeApiKernel.GetProcAddress(module, "GetNProps"));
            FindProp = Marshal.GetDelegateForFunctionPointer<TFindProp>(NativeApiKernel.GetProcAddress(module, "FindProp"));
            IsPropReadable = Marshal.GetDelegateForFunctionPointer<TIsPropReadable>(NativeApiKernel.GetProcAddress(module, "IsPropReadable"));
            IsPropWritable = Marshal.GetDelegateForFunctionPointer<TIsPropWritable>(NativeApiKernel.GetProcAddress(module, "IsPropWritable"));
            GetPropName = Marshal.GetDelegateForFunctionPointer<TGetPropName>(NativeApiKernel.GetProcAddress(module, "GetPropName"));
            GetPropVal = Marshal.GetDelegateForFunctionPointer<TGetPropVal>(NativeApiKernel.GetProcAddress(module, "GetPropVal"));
            SetPropVal = Marshal.GetDelegateForFunctionPointer<TSetPropVal>(NativeApiKernel.GetProcAddress(module, "SetPropVal"));
            GetNMethods = Marshal.GetDelegateForFunctionPointer<TGetNMethods>(NativeApiKernel.GetProcAddress(module, "GetNMethods"));
            FindMethod = Marshal.GetDelegateForFunctionPointer<TFindMethod>(NativeApiKernel.GetProcAddress(module, "FindMethod"));
            GetMethodName = Marshal.GetDelegateForFunctionPointer<TGetMethodName>(NativeApiKernel.GetProcAddress(module, "GetMethodName"));
            GetNParams = Marshal.GetDelegateForFunctionPointer<TGetNParams>(NativeApiKernel.GetProcAddress(module, "GetNParams"));
            GetParamDefValue = Marshal.GetDelegateForFunctionPointer<TGetParamDefValue>(NativeApiKernel.GetProcAddress(module, "GetParamDefValue"));
            HasRetVal = Marshal.GetDelegateForFunctionPointer<THasRetVal>(NativeApiKernel.GetProcAddress(module, "HasRetVal"));
            CallAsProc = Marshal.GetDelegateForFunctionPointer<TCallAsProc>(NativeApiKernel.GetProcAddress(module, "CallAsProc"));
            CallAsFunc = Marshal.GetDelegateForFunctionPointer<TCallAsFunc>(NativeApiKernel.GetProcAddress(module, "CallAsFunc"));
        }

        public static bool IsLinux
        {
            get => System.Environment.OSVersion.Platform == PlatformID.Unix;
        }

        public delegate void PointerDelegate(IntPtr ptr);
        public delegate void ArrayDelegate(IntPtr ptr, Int32 number);
        public delegate void OnErrorDelegate(UInt16 wcode, IntPtr source, IntPtr descr, Int32 scode);
        public delegate void OnEventDelegate(IntPtr source, IntPtr message, IntPtr data);
        public delegate void OnStatusDelegate(IntPtr status);

        public static String Str(IntPtr p)
        {
            return p != IntPtr.Zero ? Marshal.PtrToStringUni(p) : String.Empty;
        }

    }
}