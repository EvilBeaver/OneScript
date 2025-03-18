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
    [SystemEnum("НедопустимыеПодстановкиXS", "XSDisallowedSubstitutions")]
    public sealed class EnumerationXSDisallowedSubstitutions : ClrEnumWrapperCached<XmlSchemaDerivationMethod>
    {
        private EnumerationXSDisallowedSubstitutions(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Все", "All", XmlSchemaDerivationMethod.All);
            MakeValue("Ограничение", "Restriction", XmlSchemaDerivationMethod.Restriction);
            MakeValue("Подстановка", "Substitution", XmlSchemaDerivationMethod.Substitution);
            MakeValue("Расширение", "Extension", XmlSchemaDerivationMethod.Extension);
        }

        public static EnumerationXSDisallowedSubstitutions CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t,v) => new EnumerationXSDisallowedSubstitutions(t, v));
        }
    }
}
