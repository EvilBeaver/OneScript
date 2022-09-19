/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.DependencyInjection;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using ScriptEngine.Compiler;

namespace ScriptEngine.Hosting
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CompilerServiceFactory : ICompilerServiceFactory
    {
        private readonly IServiceContainer _services;

        public CompilerServiceFactory(IServiceContainer services)
        {
            _services = services;
        }

        public ICompilerFrontend CreateInstance(SymbolTable context)
        {
            var compiler = _services.Resolve<CompilerFrontend>();
            compiler.Symbols = context;
            return compiler;
        }
    }
}