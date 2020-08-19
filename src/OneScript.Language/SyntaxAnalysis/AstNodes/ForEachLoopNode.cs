/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis.AstNodes
{
    public class ForEachLoopNode : BranchingStatementNode
    {
        public ForEachLoopNode(Lexem startLexem) : base(NodeKind.ForEachLoop, startLexem)
        {
        }

        public TerminalNode IteratorVariable { get; set; }
        
        public BslSyntaxNode CollectionExpression { get; set; }
        
        public CodeBatchNode LoopBody { get; set; }
        
        protected override void OnChildAdded(BslSyntaxNode child)
        {
            /* В случае ошибки в заголовке цикла, если строится полное дерево без выброса исключений,
             парсер "перемотается" на первую понятную ему конструкцию If/Loop/Try/etc..
             В этом случае переменной или коллекции может не быть, т.к. они "перемотаются" и 
             парситься не будут. Нужно отловить этот случай, когда Batch добавляется, а элементы из заголовка цикла не 
             добавлялись.
             */
            switch (child.Kind)
            {
                case NodeKind.ForEachVariable:
                    IteratorVariable = (TerminalNode) child;
                    break;
                case NodeKind.ForEachCollection:
                    CollectionExpression = child.Children[0];
                    break;
                case NodeKind.CodeBatch:
                    if (CollectionExpression == default)
                    {
                        IteratorVariable = new TerminalNode(NodeKind.Unknown);
                        CollectionExpression = new NonTerminalNode(NodeKind.Unknown);
                    }

                    LoopBody = (CodeBatchNode) child;
                    break;
                case NodeKind.BlockEnd:
                    base.OnChildAdded(child);
                    break;
            }
        }
    }
}