/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq.Expressions;

namespace OneScript.Language.LexicalAnalysis
{
    public class LexerDetectorBuilder
    {
        public Expression<Func<char, SourceCodeIterator, bool>> DetectExpression { get; }
        public Expression HandlerExpression { get; private set; }

        public LexerDetectorBuilder(Expression<Func<char, SourceCodeIterator, bool>> detectExpression)
        {
            DetectExpression = detectExpression;
        }

        public void HandleWith(LexerState state)
        {
            HandlerExpression = Expression.Constant(state);
        }
        
        public void HandleWith(Expression<Func<SourceCodeIterator, LexerState>> selector)
        {
            HandlerExpression = Expression.Invoke(selector);
        }
    }
}