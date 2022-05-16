/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Json
{
    [EnumerationType("ЭкранированиеСимволовJSON", "JSONCharactersEscapeMode",
        TypeUUID = "A7FA438B-AC4D-4811-BB88-29F16BB9594D",
        ValueTypeUUID = "064950E9-91D3-4FF4-836D-BE6C8BF173E3")]
    public enum JSONCharactersEscapeModeEnum
    {
        [EnumValue("Нет", "None")]
        None,
        
        [EnumValue("СимволыВнеASCII", "NotASCIISymbols")]
        NotASCIISymbols,
        
        [EnumValue("СимволыВнеBMP", "SymbolsNotInBMP")]
        SymbolsNotInBMP
    }

}
