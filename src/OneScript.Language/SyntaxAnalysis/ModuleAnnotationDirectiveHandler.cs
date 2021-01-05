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
    public abstract class ModuleAnnotationDirectiveHandler : IDirectiveHandler
    {
        private bool _enabled;
        
        public virtual void OnModuleEnter(ParserContext context)
        {
            _enabled = true;
        }

        public virtual void OnModuleLeave(ParserContext context)
        {
            _enabled = false;
        }

        public bool HandleDirective(ParserContext context)
        {
            if (!DirectiveSupported(context.LastExtractedLexem.Content))
            {
                return default;
            }

            if (!_enabled)
            {
                context.AddError(LocalizedErrors.DirectiveNotSupported(context.LastExtractedLexem.Content));
                return true;
            }

            return HandleDirectiveInternal(context);
        }

        protected virtual bool HandleDirectiveInternal(ParserContext context)
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