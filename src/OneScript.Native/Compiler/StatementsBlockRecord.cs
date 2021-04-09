/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq.Expressions;

namespace OneScript.Native.Compiler
{
    public class StatementsBlockRecord
    {
        private readonly List<Expression> _statements = new List<Expression>();
        private readonly JumpInformationRecord _jumpContext;
        private readonly Stack<Expression> _buildParts = new Stack<Expression>();

        public StatementsBlockRecord()
        {
            _jumpContext = new JumpInformationRecord();
        }
        
        public StatementsBlockRecord(JumpInformationRecord jumpTargets)
        {
            _jumpContext = jumpTargets;
        }

        public LabelTarget MethodReturn => _jumpContext.MethodReturn;
        public LabelTarget LoopContinue => _jumpContext.LoopContinue;
        public LabelTarget LoopBreak => _jumpContext.LoopBreak;

        public void Add(Expression statement) => _statements.Add(statement);

        public IList<Expression> GetStatements() => _statements;

        public Stack<Expression> BuildStack => _buildParts;
    }
}