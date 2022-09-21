/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;

namespace ScriptEngine.Compiler
{
    public class NativeCompilerBackend : ICompilerBackend
    {
        private readonly ModuleCompiler _codeGen;

        public NativeCompilerBackend(IErrorSink errorSink, IServiceContainer services)
        {
            _codeGen = new ModuleCompiler(errorSink, services);
        }

        public bool GenerateDebugCode { get; set; }
        
        public bool GenerateCodeStat { get; set; }
        
        public SymbolTable Symbols { get; set; }
        
        public IExecutableModule Compile(ModuleNode parsedModule, Type classType)
        {
            return _codeGen.Compile(parsedModule.Source, parsedModule, Symbols);
        }
    }
}
