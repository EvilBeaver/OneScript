/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Xml.Schema;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    public class XSSimpleFinal : ClrEnumValueWrapper<XmlSchemaDerivationMethod>
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

                    EnumerationXSSimpleFinal enumeration = GlobalsHelper.GetEnum<EnumerationXSSimpleFinal>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSSimpleFinal CreateInstance(ITypeManager typeManager)
        {

            var type = typeManager.RegisterType(
                "ПеречислениеЗавершенностьПростогоТипаXS",
                "EnumerationXSSimpleFinal", typeof(EnumerationXSSimpleFinal));
            
            var enumValueType = typeManager.RegisterType(
                "ЗавершенностьПростогоТипаXS",
                "XSSimpleFinal", typeof(XSSimpleFinal));

            EnumerationXSSimpleFinal instance = new EnumerationXSSimpleFinal(type, enumValueType);

            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);
            instance.AddValue("Объединение", "Union", instance._valuesCache[XmlSchemaDerivationMethod.Union]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Список", "List", instance._valuesCache[XmlSchemaDerivationMethod.List]);

            return instance;
        }
    }
}
