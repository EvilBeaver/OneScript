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
        private ZipCompressionMethodEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }
        
        public static ZipCompressionMethodEnum CreateInstance()
        {
            ZipCompressionMethodEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеМетодСжатияZIP", typeof(ZipStorePathModeEnum));
            var enumValueType = TypeManager.RegisterType("МетодСжатияZIP", typeof(SelfAwareEnumValue<ZipStorePathModeEnum>));

            instance = new ZipCompressionMethodEnum(type, enumValueType);

            instance.AddValue("Копирование", new SelfAwareEnumValue<ZipCompressionMethodEnum>(instance));
            instance.AddValue("Сжатие", new SelfAwareEnumValue<ZipCompressionMethodEnum>(instance));
            
            return instance;
        }
    }
}
