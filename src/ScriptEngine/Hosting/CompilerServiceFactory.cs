/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DependencyInjection;
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
        
        public CompilerServiceFactory(
            PreprocessorHandlers handlers,
            IErrorSink errorSink,
            IDependencyResolver dependencyResolver)
        {
            _handlers = handlers;
            _errorSink = errorSink;
            _dependencyResolver = dependencyResolver;
        }

        public ICompilerService CreateInstance(ICompilerContext context)
        {
            var opts = new CompilerOptions
            {
                DependencyResolver = _dependencyResolver,
                ErrorSink = _errorSink,
                PreprocessorHandlers = _handlers
            };

            return new StackRuntimeCompilerService(opts, context);
        }
    }
}