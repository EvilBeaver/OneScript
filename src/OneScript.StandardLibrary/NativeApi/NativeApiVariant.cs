﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.InteropServices;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.NativeApi
{


    /// <summary>
    /// Трансляция значений между IValue и tVariant из состава Native API
    /// </summary>
    class NativeApiVariant: IDisposable
    {
        private readonly IntPtr variant = IntPtr.Zero;

        public IntPtr Ptr { get { return variant; } }

        public NativeApiVariant(Int32 count = 1)
        {
            variant = NativeApiProxy.CreateVariant(count);
        }

        public void Dispose()
        { 
            if (variant != IntPtr.Zero)
                NativeApiProxy.FreeVariant(variant);
        }

        public void Assign(IValue value, Int32 number = 0)
        {
            var clrObject = value.UnwrapToClrObject();
            switch (clrObject)
            {
                case string str:
                    NativeApiProxy.SetVariantStr(variant, number, str, str.Length);
                    break;
                case bool v:
                    NativeApiProxy.SetVariantBool(variant, number, value.AsBoolean());
                    break;
                case decimal num:
                    if (num % 1 == 0)
                        NativeApiProxy.SetVariantInt(variant, number, Convert.ToInt32(value.AsNumber()));
                    else
                        NativeApiProxy.SetVariantReal(variant, number, Convert.ToDouble(value.AsNumber()));
                    break;
                case BinaryDataContext binaryData:
                    NativeApiProxy.SetVariantBlob(variant, number, binaryData.Buffer, binaryData.Buffer.Length);
                    break;
                default:
                    NativeApiProxy.SetVariantEmpty(variant, number);
                    break;
            }
        }
        public static IValue Value(IntPtr variant, Int32 number = 0)
        {
            IValue value = ValueFactory.Create();
            NativeApiProxy.GetVariant(variant, number,
                (v, n) => value = ValueFactory.Create(),
                (v, n, r) => value = ValueFactory.Create(r),
                (v, n, r) => value = ValueFactory.Create((Decimal)r),
                (v, n, r) => value = ValueFactory.Create((Decimal)r),
                (v, n, r, s) => value = ValueFactory.Create(Marshal.PtrToStringUni(r, s)),
                (v, n, r, s) => {
                    byte[] buffer = new byte[s];
                    Marshal.Copy(r, buffer, 0, s);
                    value = new BinaryDataContext(buffer);
                }
            );
            return value;
        }
    }
}