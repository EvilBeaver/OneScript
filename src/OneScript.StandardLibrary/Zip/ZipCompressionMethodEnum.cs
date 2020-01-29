/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
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
        
        [EnumValue(EV_COPY_NAME, "Copy")]
        public EnumerationValue Copy
        {
            get
            {
                return this[EV_COPY_NAME];
            }
        }

        [EnumValue(EV_DEFLATE_NAME, "Deflate")]
        public EnumerationValue Deflate
        {
            get
            {
                return this[EV_DEFLATE_NAME];
            }
        }

        public static ZipCompressionMethodEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<ZipCompressionMethodEnum>((t, v) => new ZipCompressionMethodEnum(t, v));
        }
    }
}
