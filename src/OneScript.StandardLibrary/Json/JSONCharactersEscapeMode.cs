/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

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

        public static JSONCharactersEscapeModeEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<JSONCharactersEscapeModeEnum>((t, v) => new JSONCharactersEscapeModeEnum(t, v));
        }

    }

}
