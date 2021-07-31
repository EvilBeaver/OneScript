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
        
        protected ModuleAnnotationDirectiveHandler(IErrorSink errorSink) : base(errorSink)
        {
        }
        
        public override void OnModuleEnter()
        {
            _enabled = true;
            base.OnModuleEnter();
        }

        public override void OnModuleLeave()
        {
            _enabled = false;
            base.OnModuleLeave();
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
        
        protected abstract void ParseAnnotationInternal(
            ref Lexem lastExtractedLexem,
            ILexer lexer,
            ParserContext parserContext);

        public bool ParseAnnotation(ref Lexem lastExtractedLexem, ILexer lexer, ParserContext context)
        {
            if (!DirectiveSupported(lastExtractedLexem.Content))
            {
                return false;
            }

            ParseAnnotationInternal(ref lastExtractedLexem, lexer, context);
            return true;
        }
    }
}