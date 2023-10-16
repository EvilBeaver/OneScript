/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Values;

namespace OneScript
{
    public static class TypeUtils
    {
        public static bool IsNumeric(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int32:
                case TypeCode.Decimal:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.Int16:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsValue(this Type type) => typeof(BslValue).IsAssignableFrom(type);
        
        public static bool IsContext(this Type type) => typeof(IRuntimeContextInstance).IsAssignableFrom(type);
        
        public static bool IsObjectValue(this Type type) => typeof(BslObjectValue).IsAssignableFrom(type);
    }
}