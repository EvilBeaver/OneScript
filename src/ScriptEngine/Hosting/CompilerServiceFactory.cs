/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Compiler;

namespace ScriptEngine.Hosting
{
    public class CompilerServiceFactory : ICompilerServiceFactory
    {
        private readonly CompilerOptions _compilerOptions;

        public CompilerServiceFactory(CompilerOptions options)
        {
            _compilerOptions = options;
        }

        public ICompilerService CreateInstance(ICompilerContext context)
        {
            return new StackRuntimeCompilerService(_compilerOptions, context);
        }
    }
}