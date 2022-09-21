/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Sources;

namespace OneScript.Compilation
{
    public interface ICompilerFrontend
    {
        bool GenerateDebugCode { get; set; }
        
        bool GenerateCodeStat { get; set; }
        
        IList<string> PreprocessorDefinitions { get; }
        
        SymbolTable SharedSymbols { get; set; }

        SymbolScope FillSymbols(Type targetType);
        
        IErrorSink ErrorSink { get; }
        
        IExecutableModule Compile(SourceCode source, Type classType = null);
        
        IExecutableModule CompileExpression(SourceCode source);
        
        IExecutableModule CompileBatch(SourceCode source);
    }
}
