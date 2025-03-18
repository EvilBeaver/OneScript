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
    [SystemEnum("ЗавершенностьСоставногоТипаXS", "XSComplexFinal")]
    public sealed class EnumerationXSComplexFinal : ClrEnumWrapperCached<XmlSchemaDerivationMethod>
    {
        private EnumerationXSComplexFinal(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Все", "All", XmlSchemaDerivationMethod.All);
            MakeValue("Ограничение", "Restriction", XmlSchemaDerivationMethod.Restriction);
            MakeValue("Список", "List", XmlSchemaDerivationMethod.List);
        }

        public static EnumerationXSComplexFinal CreateInstance(ITypeManager typeManager)
        {
           return CreateInstance(typeManager, (t,v) => new EnumerationXSComplexFinal(t,v));
        }
    }
}
