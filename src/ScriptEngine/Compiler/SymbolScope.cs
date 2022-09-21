/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Compilation;
using OneScript.Localization;

namespace ScriptEngine.Compiler
{
    class SymbolNotFoundException : CompilerException
    {
        public SymbolNotFoundException(string symbol) 
            : base(BilingualString.Localize($"Неизвестный символ: {symbol}", $"Unknown symbol {symbol}"))
        {
            Symbol = symbol;
        }

        public string Symbol { get; }
    }

}
