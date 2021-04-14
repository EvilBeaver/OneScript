/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Types;

namespace OneScript.Native.Compiler
{
    public class BslWalkerContext
    {
        public SymbolTable Symbols { get; set; }
        
        public IErrorSink Errors { get; set; }
        
        public ModuleInformation Module { get; set; }
        
        public ITypeManager TypeManager { get; set; }
    }
}