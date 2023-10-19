using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class LabelNode : LineMarkerNode
    {
        public LabelNode(Lexem labelLexem) : base(labelLexem.Location, NodeKind.Label)
        {
            LabelName = labelLexem.Content;
        }
        
        public string LabelName { get; }
    }
}