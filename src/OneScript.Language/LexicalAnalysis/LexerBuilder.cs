/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OneScript.Language.LexicalAnalysis
{
    public class LexerBuilder
    {
        private List<LexerDetectorBuilder> _detectors = new List<LexerDetectorBuilder>();
        private static Expression nullState = Expression.Constant(default, typeof(LexerState));

        public LexerDetectorBuilder Detect(Expression<Func<char, SourceCodeIterator, bool>> detectExpression)
        {
            var detector = new LexerDetectorBuilder(detectExpression);
            _detectors.Add(detector);
            return detector;
        }

        public ILexemGenerator Build()
        {
            Expression expr;
            var charParam = Expression.Parameter(typeof(char), "cs");
            var iteratorParam = Expression.Parameter(typeof(SourceCodeIterator), "i");
            
            using (var iterator = _detectors.GetEnumerator())
            {
                var map = new Dictionary<Type, ParameterExpression>()
                {
                    {typeof(char), charParam},
                    {typeof(SourceCodeIterator), iteratorParam}
                };
                var remapper = new ParameterRemapper(map);
                expr = BuildNode(iterator, remapper);
            }


            var lambda = Expression.Lambda<Func<char, LexerState>>(expr, charParam);  
            var func = lambda.Compile();
            
            return new ExpressionBasedLexer(func);
        }

        private Expression BuildNode(IEnumerator<LexerDetectorBuilder> iterator, ParameterRemapper parameterRemapper)
        {
            if (iterator.MoveNext())
            {
                var test = parameterRemapper.Visit(iterator.Current.DetectExpression.Body);
                var truePart = Expression.Convert(iterator.Current.HandlerExpression, typeof(LexerState));
                var falsePart = BuildNode(iterator, parameterRemapper);
                return Expression.Condition(test, truePart, falsePart);
            }
            
            return nullState;
        }

        private class ParameterRemapper : ExpressionVisitor
        {
            private readonly Dictionary<Type, ParameterExpression> _map;

            public ParameterRemapper(Dictionary<Type, ParameterExpression> map)
            {
                _map = map;
            }
            
            public override Expression Visit(Expression node)
            {
                if (node?.NodeType == ExpressionType.Parameter)
                {
                    return _map[node.Type];
                }
                return base.Visit(node);
            }
        }
    }
}