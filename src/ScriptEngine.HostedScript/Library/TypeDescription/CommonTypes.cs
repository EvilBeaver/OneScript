/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Library
{
    public static class CommonTypes
    {
        public static readonly TypeDescriptor BinaryData = TypeManager.GetTypeByFrameworkType(typeof(BinaryDataContext));
        public static readonly TypeDescriptor Null = TypeManager.GetTypeByFrameworkType(typeof(NullValue));
        public static readonly TypeDescriptor Boolean = TypeDescriptor.FromDataType(DataType.Boolean);
        public static readonly TypeDescriptor String = TypeDescriptor.FromDataType(DataType.String);
        public static readonly TypeDescriptor Date = TypeDescriptor.FromDataType(DataType.Date);
        public static readonly TypeDescriptor Number = TypeDescriptor.FromDataType(DataType.Number);
        public static readonly TypeDescriptor Type = TypeDescriptor.FromDataType(DataType.Type);
        public static readonly TypeDescriptor Undefined = TypeDescriptor.FromDataType(DataType.Undefined);

        public static readonly TypeDescriptor[] Primitives = {
            CommonTypes.Boolean,
            CommonTypes.BinaryData,
            CommonTypes.String,
            CommonTypes.Date,
            CommonTypes.Null,
            CommonTypes.Number,
            CommonTypes.Type
        };
    }
}