/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public abstract class DirectiveHandlerBase : IDirectiveHandler
    {
        public DirectiveHandlerBase(IErrorSink errorSink)
        {
            ErrorSink = errorSink;
        }
        
        public IErrorSink ErrorSink { get; }

        public virtual void OnModuleEnter(ParserContext context)
        {
        }

        public virtual void OnModuleLeave(ParserContext context)
        {
        }
        
        public abstract bool HandleDirective(ref Lexem lastExtractedLexem, ILexer lexer);
    }
}