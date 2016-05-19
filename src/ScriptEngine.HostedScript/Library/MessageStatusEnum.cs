using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [SystemEnum("СтатусСообщения", "MessageStatus")]
    class MessageStatusEnum : EnumerationContext
    {
        private MessageStatusEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public static MessageStatusEnum CreateInstance()
        {
            MessageStatusEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеСтатусСообщения", typeof(MessageStatusEnum));
            var enumValueType = TypeManager.RegisterType("СтатусСообщения", typeof(CLREnumValueWrapper<EchoStatus>));

            instance = new MessageStatusEnum(type, enumValueType);

            instance.AddValue("БезСтатуса", "WithoutStatus", new CLREnumValueWrapper<EchoStatus>(instance, EchoStatus.WithoutStatus));
            instance.AddValue("Важное", "Important", new CLREnumValueWrapper<EchoStatus>(instance, EchoStatus.Important));
            instance.AddValue("Внимание", "Attention", new CLREnumValueWrapper<EchoStatus>(instance, EchoStatus.Attention));
            instance.AddValue("Информация", "Information", new CLREnumValueWrapper<EchoStatus>(instance, EchoStatus.Information));
            instance.AddValue("Обычное", "Ordinary", new CLREnumValueWrapper<EchoStatus>(instance, EchoStatus.Ordinary));
            instance.AddValue("ОченьВажное", "VeryImportant", new CLREnumValueWrapper<EchoStatus>(instance, EchoStatus.VeryImportant));

            return instance;
        }
    }
}
