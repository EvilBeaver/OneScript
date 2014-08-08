using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{

    class SymbolsEnum : EnumerationContext
    {
        public SymbolsEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
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

        private static SymbolsEnum _instance;
        public static SymbolsEnum GetInstance()
        {
            if (_instance == null)
            {
                var type = TypeManager.RegisterType("Символы", typeof(SymbolsEnum));
                var stringType = TypeDescriptor.FromDataType(DataType.String);
                _instance = new SymbolsEnum(type, stringType);

                _instance.AddValue("ПС", new SymbolsEnumValue(_instance, "\n"));
                _instance.AddValue("ВК", new SymbolsEnumValue(_instance, "\r"));
                _instance.AddValue("ВТаб", new SymbolsEnumValue(_instance, "\v"));
                _instance.AddValue("Таб", new SymbolsEnumValue(_instance, "\t"));
                _instance.AddValue("ПФ", new SymbolsEnumValue(_instance, "\f"));
                _instance.AddValue("НПП", new SymbolsEnumValue(_instance, "\u00A0"));
            }

            return _instance;
        }

    }

}
