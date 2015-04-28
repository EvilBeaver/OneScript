/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Zip
{
    [SystemEnum("МетодСжатияZIP", "ZIPCompressionMethod")]
    public class ZipCompressionMethodEnum : EnumerationContext
    {
        private const string EV_COPY_NAME = "Копирование";
        private const string EV_DEFLATE_NAME = "Сжатие";

        private ZipCompressionMethodEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }
        
        [EnumValue(EV_COPY_NAME)]
        public EnumerationValue Copy
        {
            get
            {
                return this[EV_COPY_NAME];
            }
        }

        [EnumValue(EV_DEFLATE_NAME)]
        public EnumerationValue Deflate
        {
            get
            {
                return this[EV_DEFLATE_NAME];
            }
        }

        public static ZipCompressionMethodEnum CreateInstance()
        {
            ZipCompressionMethodEnum instance;

            TypeDescriptor enumType;
            TypeDescriptor enumValType;

            EnumContextHelper.RegisterEnumType<ZipCompressionMethodEnum>(out enumType, out enumValType);

            instance = new ZipCompressionMethodEnum(enumType, enumValType);

            EnumContextHelper.RegisterValues<ZipCompressionMethodEnum>(instance);

            return instance;
        }
    }
}
