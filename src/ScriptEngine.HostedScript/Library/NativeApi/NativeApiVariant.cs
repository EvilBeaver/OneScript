using ScriptEngine.Machine;
using System;
using System.Runtime.InteropServices;

namespace ScriptEngine.HostedScript.Library.NativeApi
{
    [StructLayout(LayoutKind.Explicit)]
    public struct NativeApiVariant
    {
        private enum VarTypes
        {
            VTYPE_EMPTY = 0,
            VTYPE_NULL,
            VTYPE_I2,                   //int16_t
            VTYPE_I4,                   //int32_t
            VTYPE_R4,                   //float
            VTYPE_R8,                   //double
            VTYPE_DATE,                 //DATE (double)
            VTYPE_TM,                   //struct tm
            VTYPE_PSTR,                 //struct str    string
            VTYPE_INTERFACE,            //struct iface
            VTYPE_ERROR,                //int32_t errCode
            VTYPE_BOOL,                 //bool
            VTYPE_VARIANT,              //struct _tVariant *
            VTYPE_I1,                   //int8_t
            VTYPE_UI1,                  //uint8_t
            VTYPE_UI2,                  //uint16_t
            VTYPE_UI4,                  //uint32_t
            VTYPE_I8,                   //int64_t
            VTYPE_UI8,                  //uint64_t
            VTYPE_INT,                  //int   Depends on architecture
            VTYPE_UINT,                 //unsigned int  Depends on architecture
            VTYPE_HRESULT,              //long hRes
            VTYPE_PWSTR,                //struct wstr
            VTYPE_BLOB,                 //means in struct str binary data contain
            VTYPE_CLSID,                //UUID
            VTYPE_STR_BLOB = 0xfff,
            VTYPE_VECTOR = 0x1000,
            VTYPE_ARRAY = 0x2000,
            VTYPE_BYREF = 0x4000,    //Only with struct _tVariant *
            VTYPE_RESERVED = 0x8000,
            VTYPE_ILLEGAL = 0xffff,
            VTYPE_ILLEGALMASKED = 0xfff,
            VTYPE_TYPEMASK = 0xfff
        }

        [FieldOffset(0)] private Int32 lVal;
        [FieldOffset(0)] private Boolean bVal;
        [FieldOffset(0)] private Double dblVal;
        [FieldOffset(0)] private IntPtr pstrVal;
        [FieldOffset(4)] private Int32 strLen32;
        [FieldOffset(8)] private Int32 strLen64;
        [FieldOffset(0)] private IntPtr pwstrVal;
        [FieldOffset(4)] private Int32 wstrLen32;
        [FieldOffset(8)] private Int32 wstrLen64;
        [FieldOffset(44)] private UInt16 vt;

#pragma warning disable IDE1006 // Стили именования
        private Int32 strLen
        {
            get => IntPtr.Size == 8 ? strLen64 : strLen32;
            set { if (IntPtr.Size == 8) wstrLen64 = value; else wstrLen32 = value; }
        }

        private Int32 wstrLen
        {
            get => IntPtr.Size == 8 ? wstrLen64 : wstrLen32;
            set { if (IntPtr.Size == 8) wstrLen64 = value; else wstrLen32 = value; }
        }
#pragma warning restore IDE1006 // Стили именования

        public IValue GetValue()
        {
            switch ((VarTypes)vt)
            {
                case VarTypes.VTYPE_EMPTY: return ValueFactory.Create();
                case VarTypes.VTYPE_I2:
                case VarTypes.VTYPE_I4:
                case VarTypes.VTYPE_ERROR:
                case VarTypes.VTYPE_UI1:
                    return ValueFactory.Create(lVal);
                case VarTypes.VTYPE_BOOL:
                    return ValueFactory.Create(bVal);
                case VarTypes.VTYPE_R4:
                case VarTypes.VTYPE_R8:
                    return ValueFactory.Create((Decimal)dblVal);
                case VarTypes.VTYPE_PSTR:
                    return ValueFactory.Create(Marshal.PtrToStringAnsi(pstrVal, strLen));
                case VarTypes.VTYPE_PWSTR:
                    return ValueFactory.Create(Marshal.PtrToStringUni(pwstrVal, wstrLen));
                default:
                    return ValueFactory.Create();
            }
        }

        public void SetValue(IValue value)
        {
            switch (value.DataType) {
                case DataType.String:
                    String str = value.AsString();
                    pwstrVal = Marshal.StringToHGlobalUni(str);
                    wstrLen = str.Length;
                    vt = (UInt16)VarTypes.VTYPE_PWSTR;
                    return;
                case DataType.Boolean:
                    vt = (UInt16)VarTypes.VTYPE_BOOL;
                    bVal = value.AsBoolean();
                    return;
                case DataType.Number:
                    Decimal num = value.AsNumber();
                    if (num % 1 == 0) {
                        lVal = Convert.ToInt32(num);
                        vt = (UInt16)VarTypes.VTYPE_I4;
                    } else {
                        dblVal = Convert.ToDouble(num);
                        vt = (UInt16)VarTypes.VTYPE_R8;
                    }
                    return;
                default:
                    vt = (UInt16)VarTypes.VTYPE_EMPTY;
                    return;
            }
        }

        static public IValue GetValue(IntPtr ptr)
        {
            return Marshal.PtrToStructure<NativeApiVariant>(ptr).GetValue();
        }
    }
}
