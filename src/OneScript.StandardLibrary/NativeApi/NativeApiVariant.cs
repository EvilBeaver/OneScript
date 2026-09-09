/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Binary;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Невладеющее представление одного tVariant из состава Native API
    /// </summary>
    readonly struct NativeApiVariant
    {
        public IntPtr Ptr { get; }

        public NativeApiVariant(IntPtr ptr)
        {
            Ptr = ptr;
        }

        public void Assign(IValue value)
        {
            var clrObject = value.UnwrapToClrObject();
            switch (clrObject)
            {
                case string str:
                    NativeApiProxy.SetVariantStr(Ptr, str, str.Length);
                    break;
                case bool v:
                    NativeApiProxy.SetVariantBool(Ptr, value.AsBoolean());
                    break;
                case decimal num:
                    if (num % 1 == 0)
                        NativeApiProxy.SetVariantInt(Ptr, Convert.ToInt32(value.AsNumber()));
                    else
                        NativeApiProxy.SetVariantReal(Ptr, Convert.ToDouble(value.AsNumber()));
                    break;
                case BinaryDataContext binaryData:
                    NativeApiProxy.SetVariantBlob(Ptr, binaryData.Buffer, binaryData.Buffer.Length);
                    break;
                case DateTime dt:
                    NativeApiProxy.SetVariantTm(
                        Ptr,
                        dt.Year, dt.Month, dt.Day,
                        dt.Hour, dt.Minute, dt.Second);
                    break;
                default:
                    NativeApiProxy.SetVariantEmpty(Ptr);
                    break;
            }
        }

        /// <summary>
        /// Снимок значения в той же нормализации, что использует Assign, без маршалинга через tVariant.
        /// </summary>
        public static IValue CaptureMarshalledValue(IValue value)
        {
            var clrObject = value.UnwrapToClrObject();
            switch (clrObject)
            {
                case string str:
                    return ValueFactory.Create(str);
                case bool v:
                    return ValueFactory.Create(value.AsBoolean());
                case decimal num:
                    if (num % 1 == 0)
                        return ValueFactory.Create(Convert.ToInt32(value.AsNumber()));
                    return ValueFactory.Create(value.AsNumber());
                case BinaryDataContext binaryData:
                    return binaryData;
                case DateTime dt:
                    return ValueFactory.Create(new DateTime(
                        dt.Year, dt.Month, dt.Day,
                        dt.Hour, dt.Minute, dt.Second, DateTimeKind.Unspecified));
                default:
                    return ValueFactory.Create();
            }
        }

        public IValue GetValue()
        {
            IValue value = ValueFactory.Create();
            NativeApiProxy.GetVariant(Ptr,
                () => value = ValueFactory.Create(),
                r => value = ValueFactory.Create(r),
                r => value = ValueFactory.Create((Decimal)r),
                r => value = ValueFactory.Create((Decimal)r),
                d => {
                    try
                    {
                        value = ValueFactory.Create(DateTime.FromOADate(d));
                    }
                    catch (Exception ex)
                    {
                        throw new RuntimeException($"Некорректное значение даты VTYPE_DATE: {ex.Message}");
                    }
                },
                (year, month, day, hour, minute, second) => {
                    try
                    {
                        value = ValueFactory.Create(new DateTime(
                            year, month, day, hour, minute, second, DateTimeKind.Unspecified));
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        throw new RuntimeException($"Некорректное значение даты VTYPE_TM: {ex.Message}");
                    }
                },
                (r, s) => value = ValueFactory.Create(Marshal.PtrToStringUni(r, s)),
                (r, s) => {
                    byte[] buffer = new byte[s];
                    Marshal.Copy(r, buffer, 0, s);
                    value = new BinaryDataContext(buffer);
                }
            );
            return value;
        }
    }
}
