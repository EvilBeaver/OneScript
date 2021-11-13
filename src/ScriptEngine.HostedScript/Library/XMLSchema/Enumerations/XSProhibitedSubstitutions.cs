/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Xml.Schema;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    public class XSProhibitedSubstitutions : CLREnumValueWrapper<XmlSchemaDerivationMethod>
    {
        internal XSProhibitedSubstitutions(EnumerationXSProhibitedSubstitutions instance, XmlSchemaDerivationMethod realValue)
                   : base(instance, realValue)
        {
        }

        public static XSProhibitedSubstitutions FromNativeValue(XmlSchemaDerivationMethod native)
            => EnumerationXSProhibitedSubstitutions.FromNativeValue(native);

        public static XmlSchemaDerivationMethod ToNativeValue(XSProhibitedSubstitutions wrapper)
            => wrapper.UnderlyingValue;
    }

    [SystemEnum("ЗапрещенныеПодстановкиXS", "EnumerationXSProhibitedSubstitutions")]
    public class EnumerationXSProhibitedSubstitutions : EnumerationContext
    {

        private readonly Dictionary<XmlSchemaDerivationMethod, XSProhibitedSubstitutions> _valuesCache;

        private EnumerationXSProhibitedSubstitutions(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaDerivationMethod, XSProhibitedSubstitutions>
            {
                { XmlSchemaDerivationMethod.All, new XSProhibitedSubstitutions(this, XmlSchemaDerivationMethod.All) },
                { XmlSchemaDerivationMethod.Restriction, new XSProhibitedSubstitutions(this, XmlSchemaDerivationMethod.Restriction) },
                { XmlSchemaDerivationMethod.Extension, new XSProhibitedSubstitutions(this, XmlSchemaDerivationMethod.Extension) }
            };
        }

        internal static XSProhibitedSubstitutions FromNativeValue(XmlSchemaDerivationMethod native)
        {

            switch (native)
            {
                case XmlSchemaDerivationMethod.All:
                case XmlSchemaDerivationMethod.Restriction:
                case XmlSchemaDerivationMethod.Extension:

                    EnumerationXSProhibitedSubstitutions enumeration = GlobalsManager.GetEnum<EnumerationXSProhibitedSubstitutions>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSProhibitedSubstitutions CreateInstance()
        {

            TypeDescriptor type = TypeManager.RegisterType("ПеречислениеЗапрещенныеПодстановкиXS", typeof(EnumerationXSProhibitedSubstitutions));
            TypeDescriptor enumValueType = TypeManager.RegisterType("ЗапрещенныеПодстановкиXS", typeof(XSProhibitedSubstitutions));

            TypeManager.RegisterAliasFor(type, "EnumerationXSProhibitedSubstitutions");
            TypeManager.RegisterAliasFor(enumValueType, "XSProhibitedSubstitutions");

            EnumerationXSProhibitedSubstitutions instance = new EnumerationXSProhibitedSubstitutions(type, enumValueType);

            instance.AddValue("Расширение", "Extension", instance._valuesCache[XmlSchemaDerivationMethod.Extension]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);

            return instance;
        }
    }
}
