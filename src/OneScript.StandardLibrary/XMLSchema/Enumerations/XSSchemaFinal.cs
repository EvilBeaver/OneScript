/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [SystemEnum("ЗавершенностьСхемыXS", "XSSchemaFinal")]
    public sealed class EnumerationXSSchemaFinal : ClrEnumWrapperCached<XmlSchemaDerivationMethod>
    {
        private EnumerationXSSchemaFinal(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Все", "All", XmlSchemaDerivationMethod.All);
            MakeValue("Объединение", "Union", XmlSchemaDerivationMethod.Union);
            MakeValue("Ограничение", "Restriction", XmlSchemaDerivationMethod.Restriction);
            MakeValue("Расширение", "Extension", XmlSchemaDerivationMethod.Extension);
            MakeValue("Список", "List", XmlSchemaDerivationMethod.List);
        }
 

        public static EnumerationXSSchemaFinal CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t, v) => new EnumerationXSSchemaFinal(t, v));
        }
    }
}
