/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class PreprocessingLexer : ILexer
    {
        private readonly ILexer _baseLexer;

        public PreprocessingLexer(ILexer baseLexer)
        {
            _baseLexer = baseLexer;
        }
        
        public PreprocessorHandlers Handlers { get; set; }

        public IErrorSink ErrorSink { get; set; }
        
        public Lexem NextLexem()
        {
            Lexem lex;
            while ((lex = _baseLexer.NextLexem()).Type == LexemType.PreprocessorDirective)
            {
                var handled = Handlers.HandleDirective(ref lex, this);
                if (handled)
                {
                    return lex;
                }

                var err = LocalizedErrors.DirectiveNotSupported(lex.Content);
                err.Position = this.GetErrorPosition();
                
                ErrorSink?.AddError(err);
            }

            return lex;
        }

        public SourceCodeIterator Iterator
        {
            get => _baseLexer.Iterator;
            set => _baseLexer.Iterator = value;
        }
    }
}