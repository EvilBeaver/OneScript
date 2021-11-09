/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;

namespace ScriptEngine.HostedScript
{
    public class LanguageTypeAnnotationHandler : ModuleAnnotationDirectiveHandler
    {
        public LanguageTypeAnnotationHandler(IErrorSink errorSink) : base(errorSink)
        {
        }

        protected override bool DirectiveSupported(string directive)
        {
            return string.Equals(directive, "native", StringComparison.CurrentCultureIgnoreCase);
        }

        protected override void ParseAnnotationInternal(ref Lexem lastExtractedLexem, ILexer lexer, ParserContext parserContext)
        {
            throw new System.NotImplementedException();
        }
    }
}