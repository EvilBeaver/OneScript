/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
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
