/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.Compiler
{
    // Resharper disable CS0612
    public class LegacyCompilerFactory : ICompilerServiceFactory
    {
        private readonly IDirectiveResolver _directiveResolver;

        public LegacyCompilerFactory(IDirectiveResolver directiveResolver)
        {
            _directiveResolver = directiveResolver;
        }

        public ICompilerService CreateInstance(ICompilerContext context)
        {
            return new CompilerService(context)
            {
                DirectiveResolver = _directiveResolver
            };
        }
    }
}