/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [SystemEnum("ПробельныеСимволыXML", "XMLSpace")]
    public class XmlSpaceEnum : ClrEnumWrapper<XmlSpace>
    {
        private XmlSpaceEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            this.WrapClrValue("ПоУмолчанию", "Default", XmlSpace.Default);
            this.WrapClrValue("Сохранять", "Preserve", XmlSpace.Preserve);
        }

        public static XmlSpaceEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<XmlSpaceEnum, XmlSpace>(
                typeManager,
                (t,v) => new XmlSpaceEnum(t, v));
            
            OnInstanceCreation(instance);

            return instance;
        }
   }
}
