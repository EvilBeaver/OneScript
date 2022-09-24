/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Xml;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [SystemEnum("ТипПроверкиXML", "XMLValidationType")]
    public class XmlValidationTypeEnum : ClrEnumWrapper<ValidationType>
    {
        readonly Dictionary<ValidationType, ClrEnumValueWrapper<ValidationType>> _valuesCache = new Dictionary<ValidationType, ClrEnumValueWrapper<ValidationType>>();

        private XmlValidationTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("НетПроверки", "NoValidate", ValidationType.None);
            MakeValue("ОпределениеТипаДокумента", "DocumentTypeDefinition", ValidationType.DTD);
            MakeValue("СхемаXML", "XMLSchema", ValidationType.Schema);
        }

        private void MakeValue(string name, string alias, ValidationType enumValue)
        {
            var wrappedValue = this.WrapClrValue(name, alias, enumValue);
            _valuesCache[enumValue] = wrappedValue;
        }
        
        public override ClrEnumValueWrapper<ValidationType> FromNativeValue(ValidationType native)
        {
            if (_valuesCache.TryGetValue(native, out var val))
            {
                return val;
            }

            val = base.FromNativeValue(native);
            _valuesCache.Add(native, val);

            return val;
        }

        public static XmlValidationTypeEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<XmlValidationTypeEnum, ValidationType>(
                typeManager,
                (t,v) => new XmlValidationTypeEnum(t, v));
            
            OnInstanceCreation(instance);

            return instance;
        }
    }
}
