/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq.Expressions;

namespace OneScript.Dynamic.Compiler
{
    public class DynamicModule
    {
        public IList<ParameterExpression> Variables { get; }
        
        public IList<LambdaExpression> Methods { get; } = new List<LambdaExpression>();
    }
}