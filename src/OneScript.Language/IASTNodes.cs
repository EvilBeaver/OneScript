using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public interface IASTNode
    {
    }

    public interface IASTConditionNode : IASTNode
    {
        IASTNode Condition { get; set; }
        IASTNode TruePart { get; set; }
        IASTNode FalsePart { get; set; }
    }

    public interface IASTWhileNode : IASTNode
    {
        IASTNode Condition { get; set; }
        IASTNode Body { get; set; }
    }

    public interface IASTForLoopNode : IASTNode
    {
        IASTNode InitializerExpression { get; set; }
        IASTNode BoundExpression { get; set; }
        IASTNode Body { get; set; }
    }

    public interface IASTForEachNode : IASTNode
    {
        string ItemIdentifier { get; set; }
        IASTNode CollectionExpression { get; set; }
        IASTNode Body { get; set; }
    }

    public interface IASTTryCatchNode : IASTNode
    {
        IASTNode TryBlock { get; set; }
        IASTNode ExceptBlock { get; set; }
    }

    public struct ASTMethodParameter
    {
        public string Name;
        public bool ByValue;
        public bool IsOptional;
        public ConstDefinition DefaultValueLiteral;
    }
}
