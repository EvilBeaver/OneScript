/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Sources;
using System;
using System.Collections.Generic;

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
        
        IExecutableModule Compile(SourceCode source, IBslProcess process, Type classType = null);
        
        public IExecutableModule Compile<T>(SourceCode source, IBslProcess process, T target)
             where T : IAttachableContext;
 

         IExecutableModule CompileExpression(SourceCode source);
        
        IExecutableModule CompileBatch(SourceCode source);
    }
}
