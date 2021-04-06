/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Json
{
    [SystemEnum("ЭкранированиеСимволовJSON", "JSONCharactersEscapeMode")]
    public class JSONCharactersEscapeModeEnum : EnumerationContext
    {
        private JSONCharactersEscapeModeEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
           : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue("Нет", "None")]
        public EnumerationValue None
        {
            get
            {
                return this["Нет"];
            }
        }

        [EnumValue("СимволыВнеASCII", "NotASCIISymbols")]
        public EnumerationValue NotASCIISymbols
        {
            get
            {
                return this["СимволыВнеASCII"];
            }
        }

        [EnumValue("СимволыВнеBMP", "SymbolsNotInBMP")]
        public EnumerationValue SymbolsNotInBMP
        {
            get
            {
                return this["СимволыВнеBMP"];
            }
        }

        public static JSONCharactersEscapeModeEnum CreateInstance(ITypeManager typeManager)
        {
            return EnumContextHelper.CreateSelfAwareEnumInstance(typeManager,
                (t, v) => new JSONCharactersEscapeModeEnum(t, v));
        }

    }

}
