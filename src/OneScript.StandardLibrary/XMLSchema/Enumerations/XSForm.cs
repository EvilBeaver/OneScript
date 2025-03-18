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
    [SystemEnum("ФормаПредставленияXS", "XSForm")]
    public sealed class EnumerationXSForm : ClrEnumWrapperCached<XmlSchemaForm>
    {
        private EnumerationXSForm(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            MakeValue("Квалифицированная", "Qualified", XmlSchemaForm.Qualified);
            MakeValue("Неквалифицированная", "Unqualified", XmlSchemaForm.Unqualified);
        }

        public static EnumerationXSForm CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t,v) => new EnumerationXSForm(t,v));
        }
    }
}
