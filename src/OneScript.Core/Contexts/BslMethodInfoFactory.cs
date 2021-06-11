/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Contexts
{
    public class BslMethodInfoFactory<TMethodInfo> where TMethodInfo : BslScriptMethodInfo
    {
        private readonly Func<TMethodInfo> _factory;
        private readonly Func<BslParameterInfo> _parameterFactory;

        public BslMethodInfoFactory(Func<TMethodInfo> methodFactory) 
            : this(methodFactory, () => new BslParameterInfo())
        {
        }
        
        public BslMethodInfoFactory(Func<TMethodInfo> methodFactory, Func<BslParameterInfo> paramFactory)
        {
            _factory = methodFactory;
            _parameterFactory = paramFactory;
        }

        public BslMethodBuilder<TMethodInfo> NewMethod() => new BslMethodBuilder<TMethodInfo>(_factory(), _parameterFactory);
    }
}