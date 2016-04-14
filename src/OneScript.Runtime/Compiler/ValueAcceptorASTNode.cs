using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Compiler
{
    class ValueAcceptorASTNode : IASTNode
    {
        public OperationCode OperationCode { get; private set; }
        public int Argument { get; private set; }

        internal static ValueAcceptorASTNode LocalVariable(int varIndex)
        {
            return new ValueAcceptorASTNode()
            {
                OperationCode = OperationCode.LoadLocal,
                Argument = varIndex
            };
        }

        internal static ValueAcceptorASTNode ExternalVariable(int varIndex)
        {
            return new ValueAcceptorASTNode()
            {
                OperationCode = OperationCode.LoadVar,
                Argument = varIndex
            };
        }
    }
}
