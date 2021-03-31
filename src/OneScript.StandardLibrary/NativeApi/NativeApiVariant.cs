/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;
using OneScript.Core;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Трансляция значений между IValue и tVariant из состава Native API
    /// </summary>
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
            set
            {
                if (IntPtr.Size == 8)
                    wstrLen64 = value;
                else
                    wstrLen32 = value;
            }
        }

        private Int32 wstrLen
        {
            get => IntPtr.Size == 8 ? wstrLen64 : wstrLen32;
            set
            {
                if (IntPtr.Size == 8)
                    wstrLen64 = value;
                else
                    wstrLen32 = value;
            }
        }

#pragma warning restore IDE1006 // Стили именования

        public static Int32 Size
        {
            get => 48;
        }

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
                case VarTypes.VTYPE_BLOB:
                    byte[] buffer = new byte[strLen];
                    Marshal.Copy(pstrVal, buffer, 0, strLen);
                    return ValueFactory.Create(new BinaryDataContext(buffer));
                default:
                    return ValueFactory.Create();
            }
        }

        public void SetValue(IValue value)
        {
            switch (value.DataType)
            {
                case DataType.String:
                    String str = value.AsString();
                    pwstrVal = Marshal.StringToHGlobalUni(str);
                    wstrLen = str.Length;
                    vt = (UInt16)VarTypes.VTYPE_PWSTR;
                    return;
                case DataType.Boolean:
                    bVal = value.AsBoolean();
                    vt = (UInt16)VarTypes.VTYPE_BOOL;
                    return;
                case DataType.Number:
                    Decimal num = value.AsNumber();
                    if (num % 1 == 0)
                    {
                        lVal = Convert.ToInt32(num);
                        vt = (UInt16)VarTypes.VTYPE_I4;
                    }
                    else
                    {
                        dblVal = Convert.ToDouble(num);
                        vt = (UInt16)VarTypes.VTYPE_R8;
                    }
                    return;
                case DataType.Object when value.AsObject() is BinaryDataContext binaryData:
                    strLen = binaryData.Buffer.Length;
                    pstrVal = Marshal.AllocHGlobal(strLen);
                    Marshal.Copy(binaryData.Buffer, 0, pstrVal, strLen);
                    vt = (UInt16)VarTypes.VTYPE_BLOB;
                    return;
                default:
                    vt = (UInt16)VarTypes.VTYPE_EMPTY;
                    return;
            }
        }

        bool IsEmpty()
        {
            return (VarTypes)vt == VarTypes.VTYPE_EMPTY;
        }

        bool NotEmpty()
        {
            return (VarTypes)vt != VarTypes.VTYPE_EMPTY;
        }

        static public IValue GetValue(IntPtr ptr)
        {
            return Marshal.PtrToStructure<NativeApiVariant>(ptr).GetValue();
        }

        static public void GetValue(IValue[] values, IntPtr ptr, int count)
        {
            for (int i = 0; i < values.Length && i < count; i++)
                values[i] = GetValue(ptr + i * Size);
        }

        static public void SetValue(IntPtr ptr, IValue value)
        {
            Marshal.PtrToStructure<NativeApiVariant>(ptr).SetValue(value);
        }

        static public void SetValue(IntPtr ptr, IValue[] values, int count)
        {
            for (int i = 0; i < values.Length && i < count; i++)
            {
                var variant = new NativeApiVariant();
                variant.SetValue(values[i]);
                Marshal.StructureToPtr(variant, ptr + i * Size, false);
            }
        }

        static public void Clear(IntPtr ptr)
        {
            Marshal.PtrToStructure<NativeApiVariant>(ptr).Clear();
        }

        static public bool IsEmpty(IntPtr ptr)
        {
            return Marshal.PtrToStructure<NativeApiVariant>(ptr).IsEmpty();
        }

        static public bool NotEmpty(IntPtr ptr)
        {
            return Marshal.PtrToStructure<NativeApiVariant>(ptr).NotEmpty();
        }

        static public void Clear(IntPtr ptr, int count)
        {
            for (int i = 0; i < count; i++)
                Clear(ptr + i * Size);
        }

        public void Clear()
        {
            switch ((VarTypes)vt)
            {
                case VarTypes.VTYPE_PSTR:
                    Marshal.FreeHGlobal(pstrVal);
                    pstrVal = IntPtr.Zero;
                    strLen = 0;
                    break;
                case VarTypes.VTYPE_PWSTR:
                    Marshal.FreeHGlobal(pwstrVal);
                    pwstrVal = IntPtr.Zero;
                    wstrLen = 0;
                    break;
                default:
                    break;
            }
            vt = (UInt16)VarTypes.VTYPE_EMPTY;
        }
    }
}