/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [SystemEnum("ПробельныеСимволыXML", "XMLSpace")]
    public class XmlSpaceEnum : ClrEnumWrapperCached<XmlSpace>
    {
        private XmlSpaceEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("ПоУмолчанию", "Default", XmlSpace.Default);
            MakeValue("Сохранять", "Preserve", XmlSpace.Preserve);
        }

        public static XmlSpaceEnum CreateInstance(ITypeManager typeManager)
        {
             return CreateInstance(typeManager, (t, v) => new XmlSpaceEnum(t, v));
        }
   }
}
