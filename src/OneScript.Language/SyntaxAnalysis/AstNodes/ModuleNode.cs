using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class ModuleNode : AnnotatableNode
    {
        public ModuleNode(Lexem startLexem) : base(NodeKind.Module)
        {
            Location = startLexem.Location;
        }
    }
}