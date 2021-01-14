/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    /// <summary>
    /// Используется в os.web для создания аннотаций на уровне класса
    /// </summary>
    public abstract class ModuleAnnotationDirectiveHandler : DirectiveHandlerBase
    {
        private bool _enabled;
        
        protected ModuleAnnotationDirectiveHandler(IAstBuilder nodeBuilder, IErrorSink errorSink) : base(errorSink)
        {
            NodeBuilder = nodeBuilder;
        }
        
        protected IAstBuilder NodeBuilder { get; }
        
        public override void OnModuleEnter(ParserContext context)
        {
            _enabled = true;
            base.OnModuleEnter(context);
        }

        public override void OnModuleLeave(ParserContext context)
        {
            _enabled = false;
            base.OnModuleLeave(context);
        }

        public sealed override bool HandleDirective(ref Lexem lastExtractedLexem, ILexer lexer)
        {
            if (!DirectiveSupported(lastExtractedLexem.Content))
            {
                return default;
            }

            if (!_enabled)
            {
                ErrorSink.AddError(LocalizedErrors.DirectiveNotSupported(lastExtractedLexem.Content));
                return true;
            }

            return HandleDirectiveInternal(ref lastExtractedLexem, lexer);
        }

        protected virtual bool HandleDirectiveInternal(ref Lexem lastExtractedLexem, ILexer lexer)
        {
            return true; // не сдвигаем лексер, выдаем на уровень парсера
        }
        
        protected abstract bool DirectiveSupported(string directive);
        
        protected abstract void ParseAnnotationInternal(string content, ParserContext parserContext);

        public bool ParseAnnotation(Lexem lastExtractedLexem, ParserContext parserContext)
        {
            if (!DirectiveSupported(lastExtractedLexem.Content))
            {
                return false;
            }

            ParseAnnotationInternal(lastExtractedLexem.Content, parserContext);
            return true;
        }
    }
}