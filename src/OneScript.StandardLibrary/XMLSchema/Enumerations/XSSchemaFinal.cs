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

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    public class XSSchemaFinal : CLREnumValueWrapper<XmlSchemaDerivationMethod>
    {
        internal XSSchemaFinal(EnumerationXSSchemaFinal instance, XmlSchemaDerivationMethod realValue)
           : base(instance, realValue)
        {
        }

        public static XSSchemaFinal FromNativeValue(XmlSchemaDerivationMethod native)
            => EnumerationXSSchemaFinal.FromNativeValue(native);

        public static XmlSchemaDerivationMethod ToNativeValue(XSSchemaFinal wrapper)
            => wrapper.UnderlyingValue;
    }

    [SystemEnum("ЗавершенностьСхемыXS", "XSSchemaFinal")]
    public class EnumerationXSSchemaFinal : EnumerationContext
    {

        private readonly Dictionary<XmlSchemaDerivationMethod, XSSchemaFinal> _valuesCache;

        private EnumerationXSSchemaFinal(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            _valuesCache = new Dictionary<XmlSchemaDerivationMethod, XSSchemaFinal>
            {
                { XmlSchemaDerivationMethod.All, new XSSchemaFinal(this, XmlSchemaDerivationMethod.All) },
                { XmlSchemaDerivationMethod.Union, new XSSchemaFinal(this, XmlSchemaDerivationMethod.Union) },
                { XmlSchemaDerivationMethod.Restriction, new XSSchemaFinal(this, XmlSchemaDerivationMethod.Restriction) },
                { XmlSchemaDerivationMethod.Extension, new XSSchemaFinal(this, XmlSchemaDerivationMethod.Extension) },
                { XmlSchemaDerivationMethod.List, new XSSchemaFinal(this, XmlSchemaDerivationMethod.List) }
            };
        }

        internal static XSSchemaFinal FromNativeValue(XmlSchemaDerivationMethod native)
        {
            switch (native)
            {
                case XmlSchemaDerivationMethod.All:
                case XmlSchemaDerivationMethod.Union:
                case XmlSchemaDerivationMethod.Restriction:
                case XmlSchemaDerivationMethod.Extension:
                case XmlSchemaDerivationMethod.List:

                    EnumerationXSSchemaFinal enumeration = GlobalsManager.GetEnum<EnumerationXSSchemaFinal>();
                    return enumeration._valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSSchemaFinal CreateInstance(ITypeManager typeManager)
        {

            TypeDescriptor type = typeManager.RegisterType("EnumerationXSSchemaFinal", typeof(EnumerationXSSchemaFinal));
            TypeDescriptor enumValueType = typeManager.RegisterType("XSSchemaFinal", typeof(XSSchemaFinal));

            typeManager.RegisterAliasFor(type, "ПеречислениеЗавершенностьСхемыXS");
            typeManager.RegisterAliasFor(enumValueType, "ЗавершенностьСхемыXS");

            EnumerationXSSchemaFinal instance = new EnumerationXSSchemaFinal(type, enumValueType);

            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);
            instance.AddValue("Объединение", "Union", instance._valuesCache[XmlSchemaDerivationMethod.Union]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Расширение", "Extension", instance._valuesCache[XmlSchemaDerivationMethod.Extension]);
            instance.AddValue("Список", "List", instance._valuesCache[XmlSchemaDerivationMethod.List]);
 
            return instance;
        }
    }
}
