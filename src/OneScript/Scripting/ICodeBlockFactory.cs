using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public interface ICodeBlockFactory
    {
        void Init(CompilerContext context, Lexer lexer);
        IModuleBuilder GetModuleBuilder();
        IBlockBuilder GetVarDefinitionBuilder();
        IBlockBuilder GetMethodSectionBuilder();
        IBlockBuilder GetMethodBuilder();
        IBlockBuilder GetCodeBatchBuilder();
        IBlockBuilder GetStatementBuilder();
        IBlockBuilder GetComplexStatementBuilder();
        IBlockBuilder GetLeftExpressionBuilder();
        IBlockBuilder GetRightExpressionBuilder();
        IBlockBuilder GetProcCallBuilder();
        IBlockBuilder GetAssignmentBuilder();

    }
}
