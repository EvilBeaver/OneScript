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
            if(!_enabled)
                throw new SyntaxErrorException(
                    context.Lexer.GetErrorPosition(),
                    LocalizedErrors.DirectiveNotSupported(context.LastExtractedLexem.Content)
                );

            return HandleDirectiveInternal(context);
        }

        protected abstract bool HandleDirectiveInternal(ParserContext context);
    }
}