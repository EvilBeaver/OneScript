using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    /// <summary>
    /// Нода ошибочного синтаксиса
    /// </summary>
    public class ErrorTerminalNode : TerminalNode
    {
        public ErrorTerminalNode() : base(NodeKind.Unknown)
        {
        }

        public ErrorTerminalNode(Lexem lexem) : base(NodeKind.Unknown, lexem)
        {
        }
    }
}