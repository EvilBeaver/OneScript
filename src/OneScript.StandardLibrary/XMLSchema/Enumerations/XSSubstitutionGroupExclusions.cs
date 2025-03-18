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
    [SystemEnum("ИсключенияГруппПодстановкиXS", "XSSubstitutionGroupExclusions")]
    public sealed class EnumerationXSSubstitutionGroupExclusions : ClrEnumWrapperCached<XmlSchemaDerivationMethod>
    {
        private EnumerationXSSubstitutionGroupExclusions(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Все", "All", XmlSchemaDerivationMethod.All);
            MakeValue("Ограничение", "Restriction", XmlSchemaDerivationMethod.Restriction);
            MakeValue("Расширение", "Extension", XmlSchemaDerivationMethod.Extension);
        }

        public static EnumerationXSSubstitutionGroupExclusions CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t, v) => new EnumerationXSSubstitutionGroupExclusions(t, v));
        }
    }
}
