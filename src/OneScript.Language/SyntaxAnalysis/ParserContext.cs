/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ParserContext
    {
        private Lexem _lastLexem;

        public ILexer Lexer { get; }
        
        public IErrorSink ErrorSink { get; set; } = new ThrowingErrorSink();
        
        public Stack<BslSyntaxNode> NodeContext { get; } = new Stack<BslSyntaxNode>();
        
        public IAstBuilder NodeBuilder { get; }
        
        public PreprocessorHandlers DirectiveHandlers { get; set; }

        public Lexem LastExtractedLexem
        {
            get => _lastLexem;
            set => _lastLexem = value;
        }

        public ref Lexem NextLexem()
        {
            _lastLexem = Lexer.NextLexem();
            return ref _lastLexem;
        }
        
        public void AddError(ParseError err)
        {
            ErrorSink?.AddError(err);
        }

        public ParserContext(ILexer lexer,IAstBuilder builder)
        {
            Lexer = lexer;
            NodeBuilder = builder;
        }

        public ParserContext(ILexer lexer, IAstBuilder builder, Lexem lexem)
            :this(lexer, builder)
        {
            _lastLexem = lexem;
        }

        public ParserContext(ILexer lexer, IAstBuilder astBuilder, PreprocessorHandlers preprocessorHandlers)
            : this(lexer, astBuilder)
        {
            DirectiveHandlers = preprocessorHandlers;
        }
    }
}