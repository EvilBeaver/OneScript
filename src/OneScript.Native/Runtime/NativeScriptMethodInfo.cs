/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq.Expressions;
using OneScript.Contexts.Reflection;

namespace OneScript.Native.Runtime
{
    public class NativeScriptMethodInfo : BslScriptMethodInfo
    {
        public LambdaExpression Implementation { get; private set; }
        
        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
        }
    }
}