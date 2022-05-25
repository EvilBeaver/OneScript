/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [SystemEnum("ТипПроверкиXML", "XMLValidationType")]
    public class XMLValidationTypeEnum : EnumerationContext
    {
        readonly Dictionary<ValidationType, IValue> _valuesCache = new Dictionary<ValidationType, IValue>();

        private XMLValidationTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public IValue FromNativeValue(ValidationType native)
        {
            if (_valuesCache.TryGetValue(native, out IValue val))
            {
                return val;
            }
            else
            {
                val = this.ValuesInternal.First(x => ((CLREnumValueWrapper<ValidationType>)x).UnderlyingValue == native);
                _valuesCache.Add(native, val);
            }

            return val;
        }

        public static XMLValidationTypeEnum CreateInstance()
        {
            XMLValidationTypeEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеТипПроверкиXML", typeof(XMLValidationTypeEnum));
            var enumValueType = TypeManager.RegisterType("ТипПроверкиXML", typeof(CLREnumValueWrapper<ValidationType>));

            instance = new XMLValidationTypeEnum(type, enumValueType);

            instance.AddValue("НетПроверки", "NoValidate", new CLREnumValueWrapper<ValidationType>(instance, ValidationType.None));
            instance.AddValue("ОпределениеТипаДокумента", "DocumentTypeDefinition", new CLREnumValueWrapper<ValidationType>(instance, ValidationType.DTD));
            instance.AddValue("СхемаXML", "XMLSchema", new CLREnumValueWrapper<ValidationType>(instance, ValidationType.Schema));

            return instance;
        }
    }
}
