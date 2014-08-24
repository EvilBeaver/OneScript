using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class ByteCodeBlockFactory : ICodeBlockFactory
    {
        CompilerContext _context;
        LexemExtractor _extractor;
        ModuleBuilder _builder;

        StatementBuilder _statementBuilder;

        public void Init(CompilerContext context, Lexer lexer)
        {
            _context = context;
            _extractor = new LexemExtractor(lexer);
            _builder = new ModuleBuilder(this, _context, _extractor);
            _statementBuilder = new StatementBuilder(_builder);
        }

        public IModuleBuilder GetModuleBuilder()
        {
            return _builder;
        }

        public IBlockBuilder GetVarDefinitionBuilder()
        {
            return new VariableDefinitionBuilder(_builder);
        }

        public IBlockBuilder GetMethodSectionBuilder()
        {
            return new MethodSectionBuilder(_builder);
        }

        public IBlockBuilder GetMethodBuilder()
        {
            return new MethodBuilder(_builder);
        }

        public IBlockBuilder GetCodeBatchBuilder()
        {
            return new CodeBatchBuilder(_builder);
        }

        public IBlockBuilder GetStatementBuilder()
        {
            return _statementBuilder;
        }

        public IBlockBuilder GetComplexStatementBuilder()
        {
            return _statementBuilder;
        }

        public IBlockBuilder GetLeftExpressionBuilder()
        {
            return new AssignableExpressionBuilder(_builder);
        }

        public IBlockBuilder GetRightExpressionBuilder()
        {
            return new SourceExpressionBuilder(_builder);
        }

        public IBlockBuilder GetProcCallBuilder()
        {
            return new CallBuilder(_builder);
        }

        public IBlockBuilder GetAssignmentBuilder()
        {
            return new AssignmentBuilder(_builder);
        }
    }
}
