using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{

    [SystemEnum("Символы", "Symbols")]
    public class SymbolsEnum : EnumerationContext
    {
        private SymbolsEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            :base(typeRepresentation, valuesType)
        {

        }

        class SymbolsEnumValue : EnumerationValue
        {
            string _val;

            public SymbolsEnumValue(EnumerationContext owner, string val)
                : base(owner)
            {
                _val = val;
            }

            public override string AsString()
            {
                return _val;
            }

            public override DataType DataType
            {
                get
                {
                    return DataType.String;
                }
            }

            public override TypeDescriptor SystemType
            {
                get
                {
                    return TypeDescriptor.FromDataType(DataType);
                }
            }

            public override int CompareTo(IValue other)
            {
                return _val.CompareTo(other.AsString());
            }

            public override bool Equals(IValue other)
            {
                return _val == other.AsString();
            }
        }

        public static SymbolsEnum CreateInstance()
        {

            var type = TypeManager.RegisterType("Символы", typeof(SymbolsEnum));
            var stringType = TypeDescriptor.FromDataType(DataType.String);
            var instance = new SymbolsEnum(type, stringType);

            instance.AddValue("ПС", new SymbolsEnumValue(instance, "\n"));
            instance.AddValue("ВК", new SymbolsEnumValue(instance, "\r"));
            instance.AddValue("ВТаб", new SymbolsEnumValue(instance, "\v"));
            instance.AddValue("Таб", new SymbolsEnumValue(instance, "\t"));
            instance.AddValue("ПФ", new SymbolsEnumValue(instance, "\f"));
            instance.AddValue("НПП", new SymbolsEnumValue(instance, "\u00A0"));

            return instance;
        }

    }

}
