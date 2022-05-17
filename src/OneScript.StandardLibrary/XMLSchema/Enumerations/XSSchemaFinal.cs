/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Xml.Schema;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    public class XSSchemaFinal : ClrEnumValueWrapper<XmlSchemaDerivationMethod>
    {
        internal XSSchemaFinal(EnumerationXSSchemaFinal instance, XmlSchemaDerivationMethod realValue)
           : base(instance, realValue)
        {
        }
    }

    [SystemEnum("ЗавершенностьСхемыXS", "XSSchemaFinal")]
    public class EnumerationXSSchemaFinal : ClrEnumWrapper<XmlSchemaDerivationMethod>
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

        public override ClrEnumValueWrapper<XmlSchemaDerivationMethod> FromNativeValue(XmlSchemaDerivationMethod native)
        {
            switch (native)
            {
                case XmlSchemaDerivationMethod.All:
                case XmlSchemaDerivationMethod.Union:
                case XmlSchemaDerivationMethod.Restriction:
                case XmlSchemaDerivationMethod.Extension:
                case XmlSchemaDerivationMethod.List:
                    
                    return _valuesCache[native];

                default:
                    return null;
            }
        }

        public static EnumerationXSSchemaFinal CreateInstance(ITypeManager typeManager)
        {
            var (enumType, enumValType) = EnumContextHelper.RegisterEnumType<EnumerationXSSchemaFinal, XSSchemaFinal>(typeManager);
            
            var instance = new EnumerationXSSchemaFinal(enumType, enumValType);

            instance.AddValue("Все", "All", instance._valuesCache[XmlSchemaDerivationMethod.All]);
            instance.AddValue("Объединение", "Union", instance._valuesCache[XmlSchemaDerivationMethod.Union]);
            instance.AddValue("Ограничение", "Restriction", instance._valuesCache[XmlSchemaDerivationMethod.Restriction]);
            instance.AddValue("Расширение", "Extension", instance._valuesCache[XmlSchemaDerivationMethod.Extension]);
            instance.AddValue("Список", "List", instance._valuesCache[XmlSchemaDerivationMethod.List]);
 
            OnInstanceCreation(instance);
            
            return instance;
        }
    }
}
