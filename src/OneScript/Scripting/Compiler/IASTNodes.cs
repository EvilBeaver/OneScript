using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler
{
    public interface IASTNode
    {
    }

    public interface IASTIfNode : IASTNode
    {
        IASTNode Condition { get; set; }
        IASTNode TruePart { get; set; }
        IASTNode FalsePart { get; set; }
    }

    public struct ASTMethodParameter
    {
        public string Name;
        public bool ByValue;
        public bool IsOptional;
        public ConstDefinition DefaultValueLiteral;
    }
}
