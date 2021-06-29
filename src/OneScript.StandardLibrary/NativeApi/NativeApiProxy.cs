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
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr TGetClassObject(IntPtr module, [MarshalAs(UnmanagedType.LPWStr)] string name, OnErrorDelegate onError, OnEventDelegate onEvent, OnStatusDelegate onStatus);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TDestroyObject(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr TCreateVariant(Int32 length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TFreeVariant(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TGetNProps(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TFindProp(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string wsPropName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TIsPropReadable(IntPtr ptr, Int32 lPropNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TIsPropWritable(IntPtr ptr, Int32 lPropNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TGetPropName(IntPtr ptr, Int32 lPropNum, Int32 lPropAlias, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TGetPropVal(IntPtr ptr, Int32 lPropNum, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetPropVal(IntPtr ptr, Int32 lPropNum, IntPtr variant);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantEmpty(IntPtr ptr, Int32 num);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantBool(IntPtr ptr, Int32 num, Boolean value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantReal(IntPtr ptr, Int32 num, Double value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantInt(IntPtr ptr, Int32 num, Int32 value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantStr(IntPtr ptr, Int32 num, [MarshalAs(UnmanagedType.LPWStr)] string value, Int32 length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantPtr(IntPtr ptr, Int32 num, IntPtr value, Int32 length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TSetVariantBlob(IntPtr ptr, Int32 num, byte[] data, Int32 length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TGetVariant(IntPtr ptr, Int32 num
            , TSetVariantEmpty e
            , TSetVariantBool b
            , TSetVariantInt i
            , TSetVariantReal r
            , TSetVariantPtr s
            , TSetVariantPtr x
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TGetNMethods(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TFindMethod(IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string wsMethodName);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate void TGetMethodName(IntPtr ptr, Int32 lMethodNum, Int32 lMethodAlias, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate Int32 TGetNParams(IntPtr ptr, Int32 lMethodNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool THasParamDefValue(IntPtr ptr, Int32 lMethodNum, Int32 lParamNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TGetParamDefValue(IntPtr ptr, Int32 lMethodNum, Int32 lParamNum, PointerDelegate response);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool THasRetVal(IntPtr ptr, Int32 lMethodNum);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TCallAsProc(IntPtr ptr, Int32 lMethodNum, IntPtr value);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate bool TCallAsFunc(IntPtr ptr, Int32 lMethodNum, IntPtr value, PointerDelegate response);

        public static TGetClassObject GetClassObject;
        public static TDestroyObject DestroyObject;
        public static TCreateVariant CreateVariant;
        public static TFreeVariant FreeVariant;
        public static TGetNProps GetNProps;
        public static TFindProp FindProp;
        public static TIsPropReadable IsPropReadable;
        public static TIsPropWritable IsPropWritable;
        public static TGetPropName GetPropName;
        public static TGetPropVal GetPropVal;
        public static TSetPropVal SetPropVal;
        public static TGetVariant GetVariant;
        public static TSetVariantEmpty SetVariantEmpty;
        public static TSetVariantBool SetVariantBool;
        public static TSetVariantReal SetVariantReal;
        public static TSetVariantBlob SetVariantBlob;
        public static TSetVariantInt SetVariantInt;
        public static TSetVariantStr SetVariantStr;
        public static TGetNMethods GetNMethods;
        public static TFindMethod FindMethod;
        public static TGetMethodName GetMethodName;
        public static TGetNParams GetNParams;
        public static THasParamDefValue HasParamDefValue;
        public static TGetParamDefValue GetParamDefValue;
        public static THasRetVal HasRetVal;
        public static TCallAsProc CallAsProc;
        public static TCallAsFunc CallAsFunc;

        static NativeApiProxy()
        {
            string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string filename = System.IO.Path.GetDirectoryName(location)
                + System.IO.Path.DirectorySeparatorChar + "ScriptEngine.NativeApi"
                + (IntPtr.Size == 8 ? "64" : "32")
                + (NativeApiKernel.IsLinux ? ".so" : ".dll");
            IntPtr module = NativeApiKernel.LoadLibrary(filename);
            GetClassObject = Marshal.GetDelegateForFunctionPointer<TGetClassObject>(NativeApiKernel.GetProcAddress(module, "GetClassObject"));
            DestroyObject = Marshal.GetDelegateForFunctionPointer<TDestroyObject>(NativeApiKernel.GetProcAddress(module, "DestroyObject"));
            CreateVariant = Marshal.GetDelegateForFunctionPointer<TCreateVariant>(NativeApiKernel.GetProcAddress(module, "CreateVariant"));
            FreeVariant = Marshal.GetDelegateForFunctionPointer<TFreeVariant>(NativeApiKernel.GetProcAddress(module, "FreeVariant"));
            GetNProps = Marshal.GetDelegateForFunctionPointer<TGetNProps>(NativeApiKernel.GetProcAddress(module, "GetNProps"));
            FindProp = Marshal.GetDelegateForFunctionPointer<TFindProp>(NativeApiKernel.GetProcAddress(module, "FindProp"));
            IsPropReadable = Marshal.GetDelegateForFunctionPointer<TIsPropReadable>(NativeApiKernel.GetProcAddress(module, "IsPropReadable"));
            IsPropWritable = Marshal.GetDelegateForFunctionPointer<TIsPropWritable>(NativeApiKernel.GetProcAddress(module, "IsPropWritable"));
            GetPropName = Marshal.GetDelegateForFunctionPointer<TGetPropName>(NativeApiKernel.GetProcAddress(module, "GetPropName"));
            GetPropVal = Marshal.GetDelegateForFunctionPointer<TGetPropVal>(NativeApiKernel.GetProcAddress(module, "GetPropVal"));
            SetPropVal = Marshal.GetDelegateForFunctionPointer<TSetPropVal>(NativeApiKernel.GetProcAddress(module, "SetPropVal"));
            SetVariantEmpty = Marshal.GetDelegateForFunctionPointer<TSetVariantEmpty>(NativeApiKernel.GetProcAddress(module, "SetVariantEmpty"));
            SetVariantBool = Marshal.GetDelegateForFunctionPointer<TSetVariantBool>(NativeApiKernel.GetProcAddress(module, "SetVariantBool"));
            SetVariantReal = Marshal.GetDelegateForFunctionPointer<TSetVariantReal>(NativeApiKernel.GetProcAddress(module, "SetVariantReal"));
            SetVariantBlob = Marshal.GetDelegateForFunctionPointer<TSetVariantBlob>(NativeApiKernel.GetProcAddress(module, "SetVariantBlob"));
            SetVariantInt = Marshal.GetDelegateForFunctionPointer<TSetVariantInt>(NativeApiKernel.GetProcAddress(module, "SetVariantInt"));
            SetVariantStr = Marshal.GetDelegateForFunctionPointer<TSetVariantStr>(NativeApiKernel.GetProcAddress(module, "SetVariantStr"));
            GetVariant = Marshal.GetDelegateForFunctionPointer<TGetVariant>(NativeApiKernel.GetProcAddress(module, "GetVariant"));
            GetPropVal = Marshal.GetDelegateForFunctionPointer<TGetPropVal>(NativeApiKernel.GetProcAddress(module, "GetPropVal"));
            GetNMethods = Marshal.GetDelegateForFunctionPointer<TGetNMethods>(NativeApiKernel.GetProcAddress(module, "GetNMethods"));
            FindMethod = Marshal.GetDelegateForFunctionPointer<TFindMethod>(NativeApiKernel.GetProcAddress(module, "FindMethod"));
            GetMethodName = Marshal.GetDelegateForFunctionPointer<TGetMethodName>(NativeApiKernel.GetProcAddress(module, "GetMethodName"));
            GetNParams = Marshal.GetDelegateForFunctionPointer<TGetNParams>(NativeApiKernel.GetProcAddress(module, "GetNParams"));
            HasParamDefValue = Marshal.GetDelegateForFunctionPointer<THasParamDefValue>(NativeApiKernel.GetProcAddress(module, "HasParamDefValue"));
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