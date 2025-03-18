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
    [SystemEnum("ЗапрещенныеПодстановкиXS", "XSProhibitedSubstitutions")]
    public sealed class EnumerationXSProhibitedSubstitutions : ClrEnumWrapperCached<XmlSchemaDerivationMethod>
    {
        private EnumerationXSProhibitedSubstitutions(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Все", "All", XmlSchemaDerivationMethod.All);
            MakeValue("Ограничение", "Restriction", XmlSchemaDerivationMethod.Restriction);
            MakeValue("Расширение", "Extension", XmlSchemaDerivationMethod.Extension);
        }

        public static EnumerationXSProhibitedSubstitutions CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t, v) => new EnumerationXSProhibitedSubstitutions(t, v));
        }
    }
}
