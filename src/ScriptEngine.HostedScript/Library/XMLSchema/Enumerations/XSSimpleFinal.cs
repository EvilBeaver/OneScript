using System.Collections.Generic;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public class XSSimpleFinal : CLREnumValueWrapper<XmlSchemaDerivationMethod>
    {
        internal XSSimpleFinal(EnumerationXSSimpleFinal instance, XmlSchemaDerivationMethod realValue)
          : base(instance, realValue)
        {
        }

        public static XSSimpleFinal FromNativeValue(XmlSchemaDerivationMethod native)
           => EnumerationXSSimpleFinal.FromNativeValue(native);

        public static XmlSchemaDerivationMethod ToNativeValue(XSSimpleFinal wrapper)
             => wrapper.UnderlyingValue;
    }

    [SystemEnum("ЗавершенностьПростогоТипаXS", "XSSimpleFinal")]
    public class EnumerationXSSimpleFinal : EnumerationContext
    {
        private readonly Dictionary<XmlSchemaDerivationMethod, XSSimpleFinal> _valuesCache;

        private EnumerationXSSimpleFinal(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaDerivationMethod, XSSimpleFinal>
            {
                { XmlSchemaDerivationMethod.All, new XSSimpleFinal(this, XmlSchemaDerivationMethod.All) },
                { XmlSchemaDerivationMethod.Union, new XSSimpleFinal(this, XmlSchemaDerivationMethod.Union) },
                { XmlSchemaDerivationMethod.Restriction, new XSSimpleFinal(this, XmlSchemaDerivationMethod.Restriction) },
                { XmlSchemaDerivationMethod.List, new XSSimpleFinal(this, XmlSchemaDerivationMethod.List) }
            };
        }

        public static XSSimpleFinal FromNativeValue(XmlSchemaDerivationMethod native)
        {
            switch (native)
            {
                case XmlSchemaDerivationMethod.All:
                case XmlSchemaDerivationMethod.Union:
                case XmlSchemaDerivationMethod.Restriction:
                case XmlSchemaDerivationMethod.List:

                    EnumerationXSSimpleFinal enumeration = GlobalsManager.GetEnum<EnumerationXSSimpleFinal>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSSimpleFinal CreateInstance()
        {

            TypeDescriptor type = TypeManager.RegisterType("EnumerationXSSimpleFinal", typeof(EnumerationXSSimpleFinal));
            TypeDescriptor enumValueType = TypeManager.RegisterType("XSSimpleFinal", typeof(XSSimpleFinal));

            TypeManager.RegisterAliasFor(type, "ПеречислениеЗавершенностьПростогоТипаXS");
            TypeManager.RegisterAliasFor(enumValueType, "ЗавершенностьПростогоТипаXS");

            EnumerationXSSimpleFinal instance = new EnumerationXSSimpleFinal(type, enumValueType);

            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);
            instance.AddValue("Объединение", "Union", instance._valuesCache[XmlSchemaDerivationMethod.Union]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Список", "List", instance._valuesCache[XmlSchemaDerivationMethod.List]);

            return instance;
        }
    }
}