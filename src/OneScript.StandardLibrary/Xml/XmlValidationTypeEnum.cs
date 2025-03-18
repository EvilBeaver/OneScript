/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Xml;
using OneScript.Contexts.Enums;
using OneScript.StandardLibrary.XMLSchema.Enumerations;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [SystemEnum("ТипПроверкиXML", "XMLValidationType")]
    public class XmlValidationTypeEnum : ClrEnumWrapperCached<ValidationType>
    {
        private XmlValidationTypeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("НетПроверки", "NoValidate", ValidationType.None);
            MakeValue("ОпределениеТипаДокумента", "DocumentTypeDefinition", ValidationType.DTD);
            MakeValue("СхемаXML", "XMLSchema", ValidationType.Schema);
        }

        public static XmlValidationTypeEnum CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t, v) => new XmlValidationTypeEnum(t, v));
        }
    }
}
