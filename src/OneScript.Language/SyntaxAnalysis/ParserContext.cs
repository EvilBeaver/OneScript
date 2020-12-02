using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class ParserContext
    {
        public ILexer Lexer { get; }
        
        public Stack<BslSyntaxNode> NodeContext { get; }
        
        public IAstBuilder NodeBuilder { get; }
        
        public Lexem LastExtractedLexem { get; set; }

        internal ParserContext(ILexer lexer, Stack<BslSyntaxNode> nodeContext, IAstBuilder builder, Lexem lastLexem)
        {
            Lexer = lexer;
            NodeContext = nodeContext;
            NodeBuilder = builder;
            LastExtractedLexem = lastLexem;
        }
    }
}