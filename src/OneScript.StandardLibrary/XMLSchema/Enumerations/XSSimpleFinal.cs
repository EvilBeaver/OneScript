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
    [SystemEnum("ЗавершенностьПростогоТипаXS", "XSSimpleFinal")]
    public sealed class EnumerationXSSimpleFinal : ClrEnumWrapperCached<XmlSchemaDerivationMethod>
    {
        private EnumerationXSSimpleFinal(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
            MakeValue("Все", "All", XmlSchemaDerivationMethod.All);
            MakeValue("Объединение", "Union", XmlSchemaDerivationMethod.Union);
            MakeValue("Ограничение", "Restriction", XmlSchemaDerivationMethod.Restriction);
            MakeValue("Список", "List", XmlSchemaDerivationMethod.List);
        }

        public static EnumerationXSSimpleFinal CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t, v) => new EnumerationXSSimpleFinal(t, v));
        }
    }
}
