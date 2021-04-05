using System.Collections.Generic;
using System.Linq.Expressions;

namespace OneScript.Native.Compiler
{
    public interface IBlockExpressionGenerator
    {

        void Add(Expression item);
        Expression Block();
    }

    public interface ILoopBlockExpressionGenerator : IBlockExpressionGenerator
    {
        void AddBreakExpression();
        void AddContinueExpression();
    }

    public class SimpleBlockExpressionGenerator : IBlockExpressionGenerator
    {
        readonly List<Expression> Statements = new List<Expression>();

        public void Add(Expression item)
        {
            Statements.Add(item);
        }

        public Expression Block()
        {
            return Expression.Block(Statements);
        }
    }

    public class IfThenBlockGenerator : IBlockExpressionGenerator
    {
        class IfThenElement
        {
            public readonly List<Expression> Condition = new List<Expression>();
            public readonly List<Expression> Body = new List<Expression>();
        }

        private readonly Stack<IfThenElement> _conditionalBlocks = new Stack<IfThenElement>();
        private readonly List<Expression> _elseBlock = new List<Expression>();

        private IfThenElement _currentElement;
        private List<Expression> _statements;
        
        public void Add(Expression item)
        {
            _statements.Add(item);
        }

        public Expression Block()
        {
            var top = _conditionalBlocks.Pop();
            var block = Expression.IfThenElse(
                Expression.Block(top.Condition), 
                Expression.Block(top.Body), 
                Expression.Block(_elseBlock));

            while (_conditionalBlocks.Count > 0)
            {
                var next = _conditionalBlocks.Pop();
                block = Expression.IfThenElse(
                    Expression.Block(top.Condition), 
                    Expression.Block(top.Body), 
                    block);
            }

            return block;
        }

        public void StartCondition()
        {
            _currentElement = new IfThenElement();
            _statements = _currentElement.Condition;
            _conditionalBlocks.Push(_currentElement);
        }

        public void StartBody()
        {
            _statements = _currentElement.Body;
        }

        public void StartElseBody()
        {
            _statements = _elseBlock;
        }
    }
    
    public class WhileBlockExpressionGenerator : ILoopBlockExpressionGenerator
    {
        private readonly List<Expression> _conditionStatements = new List<Expression>();
        private readonly List<Expression> _bodyStatements = new List<Expression>();

        private List<Expression> _statements = null;

        private readonly LabelTarget _continueLabel = Expression.Label(typeof(void));
        private readonly LabelTarget _breakLabel = Expression.Label(typeof(void));


        
        public void Add(Expression item)
        {
            _statements.Add(item);
        }

        public Expression Block()
        {
            var result = new List<Expression>();
            
            result.Add(Expression.Label(_continueLabel));
            result.Add(Expression.IfThen(
                Expression.Not(Expression.Block(_conditionStatements)), 
                Expression.Label(_breakLabel)));
            result.AddRange(_bodyStatements);
            result.Add(Expression.Label(_breakLabel));
            
            return Expression.Loop(Expression.Block(result), _breakLabel, _continueLabel);
        }

        public void StartCondition()
        {
            _statements = _conditionStatements;
        }

        public void StartBody()
        {
            _statements = _bodyStatements;
        }

        public void AddBreakExpression()
        {
            Add(Expression.Break(_breakLabel));
        }

        public void AddContinueExpression()
        {
            Add(Expression.Continue(_continueLabel));
        }
    }

    public class ForBlockExpressionGenerator : ILoopBlockExpressionGenerator
    {
        private readonly List<Expression> _bodyStatements = new List<Expression>();

        public Expression IteratorExpression { get; set; }
        public Expression InitialValue { get; set; }
        public Expression UpperLimit { get; set; }

        private readonly LabelTarget _continueLabel = Expression.Label(typeof(void));
        private readonly LabelTarget _breakLabel = Expression.Label(typeof(void));
        private readonly LabelTarget _loopLabel = Expression.Label(typeof(void));
       
        public void Add(Expression item)
        {
            _bodyStatements.Add(item);
        }

        public Expression Block()
        {
            var result = new List<Expression>();
            result.Add(Expression.Assign(IteratorExpression, InitialValue)); // TODO: MakeAssign ?
            var finalVar = Expression.Variable(typeof(decimal)); // TODO: BslNumericValue ?
            result.Add(Expression.Assign(finalVar, InitialValue));
            
            result.Add(Expression.Label(_loopLabel));
            result.Add(Expression.IfThen(
                Expression.GreaterThan(IteratorExpression, InitialValue), 
                Expression.Break(_breakLabel)));
            
            result.AddRange(_bodyStatements);
            
            result.Add(Expression.Label(_continueLabel));
            result.Add(Expression.Increment(IteratorExpression));
            result.Add(Expression.Goto(_loopLabel));
            result.Add(Expression.Label(_breakLabel));

            return Expression.Loop(Expression.Block(result), _breakLabel, _continueLabel);
        }

        public void AddBreakExpression()
        {
            Add(Expression.Break(_breakLabel));
        }

        public void AddContinueExpression()
        {
            Add(Expression.Continue(_continueLabel));
        }
    }
}