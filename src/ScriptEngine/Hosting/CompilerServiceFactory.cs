/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
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
        private readonly PreprocessorHandlers _handlers;
        private readonly IErrorSink _errorSink;
        private readonly IDependencyResolver _dependencyResolver;
        private readonly IServiceContainer _services;

        public CompilerServiceFactory(
            PreprocessorHandlers handlers,
            IErrorSink errorSink,
            IDependencyResolver dependencyResolver,
            IServiceContainer services)
        {
            _handlers = handlers;
            _errorSink = errorSink;
            _dependencyResolver = dependencyResolver;
            _services = services;
        }

        public ICompilerService CreateInstance(SymbolTable context)
        {
            var obsoleteCtx = new CompilerContext();
            for (int i = 0; i < context.ScopeCount; i++)
            {
                var scope = context.GetScope(i);
                obsoleteCtx.PushScope(scope);
            }

            return CreateInstance(obsoleteCtx);
        }
        
        public ICompilerService CreateInstance(ICompilerContext context)
        {
            var opts = new CompilerOptions
            {
                DependencyResolver = _dependencyResolver,
                ErrorSink = _errorSink,
                PreprocessorHandlers = _handlers
            };

            return new DefaultCompilerService(opts, context, _services);
        }
    }
}