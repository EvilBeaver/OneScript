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
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    public class XSSubstitutionGroupExclusions : ClrEnumValueWrapper<XmlSchemaDerivationMethod>
    {
        internal XSSubstitutionGroupExclusions(EnumerationXSSubstitutionGroupExclusions instance, XmlSchemaDerivationMethod realValue)
            : base(instance, realValue)
        {
        }

        public static XSSubstitutionGroupExclusions FromNativeValue(XmlSchemaDerivationMethod native)
            => EnumerationXSSubstitutionGroupExclusions.FromNativeValue(native);

        public static XmlSchemaDerivationMethod ToNativeValue(XSSubstitutionGroupExclusions wrapper)
            => wrapper.UnderlyingValue;
    }

    [SystemEnum("ИсключенияГруппПодстановкиXS", "XSSubstitutionGroupExclusions")]
    public class EnumerationXSSubstitutionGroupExclusions : EnumerationContext
    {

        private readonly Dictionary<XmlSchemaDerivationMethod, XSSubstitutionGroupExclusions> _valuesCache;

        private EnumerationXSSubstitutionGroupExclusions(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaDerivationMethod, XSSubstitutionGroupExclusions>
            {
                { XmlSchemaDerivationMethod.All, new XSSubstitutionGroupExclusions(this, XmlSchemaDerivationMethod.All) },
                { XmlSchemaDerivationMethod.Restriction, new XSSubstitutionGroupExclusions(this, XmlSchemaDerivationMethod.Restriction) },
                { XmlSchemaDerivationMethod.Extension, new XSSubstitutionGroupExclusions(this, XmlSchemaDerivationMethod.Extension) }
            };
        }

        internal static XSSubstitutionGroupExclusions FromNativeValue(XmlSchemaDerivationMethod native)
        {

            switch (native)
            {
                case XmlSchemaDerivationMethod.All:
                case XmlSchemaDerivationMethod.Restriction:
                case XmlSchemaDerivationMethod.Extension:

                    EnumerationXSSubstitutionGroupExclusions enumeration = GlobalsHelper.GetEnum<EnumerationXSSubstitutionGroupExclusions>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSSubstitutionGroupExclusions CreateInstance(ITypeManager typeManager)
        {

            var type = typeManager.RegisterType(
                "ПеречислениеИсключенияГруппПодстановкиXS",
                "EnumerationXSSubstitutionGroupExclusions", typeof(EnumerationXSSubstitutionGroupExclusions));
            var enumValueType = typeManager.RegisterType(
                "ИсключенияГруппПодстановкиXS",
                "XSSubstitutionGroupExclusions", typeof(XSSubstitutionGroupExclusions));

            EnumerationXSSubstitutionGroupExclusions instance = new EnumerationXSSubstitutionGroupExclusions(type, enumValueType);

            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Расширение", "Extension", instance._valuesCache[XmlSchemaDerivationMethod.Extension]);

            return instance;
        }
    }
}
