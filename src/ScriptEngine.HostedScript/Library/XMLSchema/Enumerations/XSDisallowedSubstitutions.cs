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
    public class XSDisallowedSubstitutions : CLREnumValueWrapper<XmlSchemaDerivationMethod>
    {
        internal XSDisallowedSubstitutions(EnumerationXSDisallowedSubstitutions instance, XmlSchemaDerivationMethod realValue) 
            : base(instance, realValue)
        {
        }

        public static XSDisallowedSubstitutions FromNativeValue(XmlSchemaDerivationMethod native) 
            => EnumerationXSDisallowedSubstitutions.FromNativeValue(native);

        public static XmlSchemaDerivationMethod ToNativeValue(XSDisallowedSubstitutions wrapper) 
            => wrapper.UnderlyingValue;
    }

    [SystemEnum("НедопустимыеПодстановкиXS", "XSDisallowedSubstitutions")]
    public class EnumerationXSDisallowedSubstitutions : EnumerationContext
    {

        private readonly Dictionary<XmlSchemaDerivationMethod, XSDisallowedSubstitutions> _valuesCache;

        private EnumerationXSDisallowedSubstitutions(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaDerivationMethod, XSDisallowedSubstitutions>
            {
                { XmlSchemaDerivationMethod.All, new XSDisallowedSubstitutions(this, XmlSchemaDerivationMethod.All) },
                { XmlSchemaDerivationMethod.Restriction, new XSDisallowedSubstitutions(this, XmlSchemaDerivationMethod.Restriction) },
                { XmlSchemaDerivationMethod.Substitution, new XSDisallowedSubstitutions(this, XmlSchemaDerivationMethod.Substitution) },
                { XmlSchemaDerivationMethod.Extension, new XSDisallowedSubstitutions(this, XmlSchemaDerivationMethod.Extension) }
            };
        }

        internal static XSDisallowedSubstitutions FromNativeValue(XmlSchemaDerivationMethod native)
        {
            
            switch (native)
            {
                case XmlSchemaDerivationMethod.All:
                case XmlSchemaDerivationMethod.Restriction:
                case XmlSchemaDerivationMethod.Substitution:
                case XmlSchemaDerivationMethod.Extension:

                    EnumerationXSDisallowedSubstitutions enumeration = GlobalsManager.GetEnum<EnumerationXSDisallowedSubstitutions>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSDisallowedSubstitutions CreateInstance()
        {

            TypeDescriptor type = TypeManager.RegisterType("ПеречислениеНедопустимыеПодстановкиXS", typeof(EnumerationXSDisallowedSubstitutions));
            TypeDescriptor enumValueType = TypeManager.RegisterType("НедопустимыеПодстановкиXS",   typeof(XSDisallowedSubstitutions));

            TypeManager.RegisterAliasFor(type, "EnumerationXSDisallowedSubstitutions"); 
            TypeManager.RegisterAliasFor(enumValueType, "XSDisallowedSubstitutions");

            EnumerationXSDisallowedSubstitutions instance = new EnumerationXSDisallowedSubstitutions(type, enumValueType);

            instance.AddValue("Расширение", "Extension", instance._valuesCache[XmlSchemaDerivationMethod.Extension]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Подстановка", "Substitution", instance._valuesCache[XmlSchemaDerivationMethod.Substitution]);
            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);

            return instance;
        }
    }
}
