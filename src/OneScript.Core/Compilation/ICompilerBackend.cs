/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/


using System;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;

namespace OneScript.Compilation
{
    public interface ICompilerBackend
    {
        bool GenerateDebugCode { get; set; }
        
        bool GenerateCodeStat { get; set; }
        
        public SymbolTable Symbols { get; set; }
        
        IExecutableModule Compile(ModuleNode parsedModule, Type classType);
    }
}