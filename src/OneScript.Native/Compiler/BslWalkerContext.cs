/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DependencyInjection;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Sources;

namespace OneScript.Native.Compiler
{
    public class BslWalkerContext
    {
        public SymbolTable Symbols { get; set; }
        
        public IErrorSink Errors { get; set; }
        
        public SourceCodeIterator CodeIterator { get; set; }
        
        public IServiceContainer Services { get; set; }
    }
}