/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml.Schema;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    internal class XMLSchemaSerializer
    {
        public static IXSComponent CreateInstance(XmlSchemaObject xmlSchemaObject)
        {
            if (xmlSchemaObject is XmlSchemaExternal xmlExternal)
                return CreateIXSDirective(xmlExternal);

            else
                return null;
        }

        private static IXSDirective CreateIXSDirective(XmlSchemaExternal xmlSchemaExternal)
        {
            if (xmlSchemaExternal is XmlSchemaImport import)
                return new XSImport(import);

            else if (xmlSchemaExternal is XmlSchemaInclude include)
                return new XSInclude(include);

            else if (xmlSchemaExternal is XmlSchemaRedefine redefine)
                return new XSRedefine(redefine);

            else
                throw RuntimeException.InvalidArgumentType();
        }
    }
}
