using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using ScriptEngine.Machine.Contexts;

namespace OneScript.Native.Compiler
{
    public interface IBlockExpressionGenerator
    {
        void Add(Expression item);
        Expression Block();
    }

    public interface ILoopBlockExpressionGenerator : IBlockExpressionGenerator
    {
        LabelTarget BreakLabel { get; }
        LabelTarget ContinueLabel { get; }
    }

    public static class GeneratorHelper
    {
        public static Expression OneOrBlock(this IList<Expression> list)
        {
            if (list.Count == 1) return list[0];
            return Expression.Block(list);
        }
    }

    public class SimpleBlockExpressionGenerator : IBlockExpressionGenerator
    {
        readonly List<Expression> _statements = new List<Expression>();

        public void Add(Expression item)
        {
            _statements.Add(item);
        }

        public Expression Block()
        {
            return _statements.OneOrBlock();
        }
    }

    public class IfThenBlockGenerator : IBlockExpressionGenerator
    {
        class IfThenElement
        {
            public Expression Condition;
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
            var block = _elseBlock.Count == 0
                ? Expression.IfThen(
                    top.Condition,
                    top.Body.OneOrBlock())
                : Expression.IfThenElse(
                    top.Condition,
                    top.Body.OneOrBlock(),
                    _elseBlock.OneOrBlock());

            while (_conditionalBlocks.Count > 0)
            {
                top = _conditionalBlocks.Pop();
                block = Expression.IfThenElse(
                    top.Condition, 
                    top.Body.OneOrBlock(), 
                    block);
            }

            return block;
        }

        public void StartCondition(Expression condition)
        {
            _currentElement = new IfThenElement();
            _currentElement.Condition = condition;
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
        private Expression _condition;
        private readonly List<Expression> _bodyStatements = new List<Expression>();

        public void Add(Expression item)
        {
            _bodyStatements.Add(item);
        }

        public Expression Block()
        {
            var result = new List<Expression>();
            
            result.Add(Expression.IfThen(
                Expression.Not(_condition), 
                Expression.Break(BreakLabel)));
            result.AddRange(_bodyStatements);

            return Expression.Loop(Expression.Block(result), BreakLabel, ContinueLabel);
        }

        public void StartCondition(Expression condition)
        {
            _condition = condition;
        }

        public LabelTarget BreakLabel { get; } = Expression.Label(typeof(void));

        public LabelTarget ContinueLabel { get; } = Expression.Label(typeof(void));
    }

    public class ForBlockExpressionGenerator : ILoopBlockExpressionGenerator
    {
        private readonly List<Expression> _bodyStatements = new List<Expression>();

        public Expression IteratorExpression { get; set; }
        public Expression InitialValue { get; set; }
        public Expression UpperLimit { get; set; }

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
            result.Add(Expression.Assign(finalVar, UpperLimit));// TODO: MakeAssign ?
            
            var loop = new List<Expression>();
            loop.Add(Expression.IfThen(
                Expression.GreaterThan(IteratorExpression, finalVar), 
                Expression.Break(BreakLabel)));
            
            loop.AddRange(_bodyStatements);
            
            loop.Add(Expression.Label(ContinueLabel));
            loop.Add(Expression.PreIncrementAssign(IteratorExpression));
            
            result.Add(Expression.Loop(Expression.Block(loop), BreakLabel));
            
            return Expression.Block(new[] {finalVar}, result);
        }

        public LabelTarget BreakLabel { get; } = Expression.Label(typeof(void));

        public LabelTarget ContinueLabel { get; } = Expression.Label(typeof(void));
    }

    public class ForEachBlockExpressionGenerator : ILoopBlockExpressionGenerator
    {
        private readonly List<Expression> _bodyStatements = new List<Expression>();

        public Expression EnumeratorExpression { get; set; }
        public Expression Iterator { get; set; }

        public void Add(Expression item)
        {
            _bodyStatements.Add(item);
        }

        public Expression Block()
        {
            var collectionType = typeof(ICollectionContext);
            var getManagedIteratorMethod = collectionType.GetMethod("GetManagedIterator");
            var moveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");
            var collectionVar = Expression.Variable(collectionType);
            
            var result = new List<Expression>();
            result.Add(Expression.Assign(collectionVar,
                Expression.TypeAs(EnumeratorExpression, typeof(ICollectionContext))));
            var enumeratorVar = Expression.Variable(typeof(CollectionEnumerator));
            result.Add(Expression.Assign(enumeratorVar, 
                Expression.Call(collectionVar, getManagedIteratorMethod)));

            var loop = new List<Expression>();

            loop.Add(Expression.Assign(Iterator, Expression.Property(enumeratorVar, "Current")));
            loop.AddRange(_bodyStatements);

            result.Add(Expression.Loop(
                Expression.IfThenElse(
                    Expression.Equal(Expression.Call(enumeratorVar, moveNextMethod), Expression.Constant(true)),
                    Expression.Block(loop),
                    Expression.Break(BreakLabel)),
                BreakLabel, ContinueLabel));

            return Expression.Block(new[] {collectionVar, enumeratorVar}, result);
        }

        public LabelTarget BreakLabel { get; } = Expression.Label(typeof(void));

        public LabelTarget ContinueLabel { get; } = Expression.Label(typeof(void));
    }

    public class TryBlockExpressionGenerator : IBlockExpressionGenerator
    {
        private readonly List<Expression> _tryStatements = new List<Expression>();
        private readonly List<Expression> _catchStatements = new List<Expression>();

        private List<Expression> _statements;
        
        public void Add(Expression item)
        {
            _statements.Add(item);
        }

        public void StartCatchBlock()
        {
            _statements = _catchStatements;
        }

        public void StartTryBlock()
        {
            _statements = _tryStatements;
        }

        public Expression Block()
        {
            return Expression.TryCatch(_tryStatements.OneOrBlock(),
                Expression.Catch(typeof(Exception), _catchStatements.OneOrBlock()));
        }
    }
}