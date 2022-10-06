/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using OneScript.Contexts;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    public class BslNativeMethodInfo : BslScriptMethodInfo
    {
        private CallableMethod _callable;

        public BslNativeMethodInfo()
        {
            _callable = new CallableMethod(this);
        }
        
        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
            _callable.Compile();
        }

        private CallableMethod CreateCallable()
        {
            return new CallableMethod(this);
        }

        internal CallableMethod GetCallable() => _callable;

        public LambdaExpression Implementation { get; private set; }

        public bool IsInstance { get; internal set; }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            var bslArguments = new List<BslValue>(parameters.Length);
            bslArguments.AddRange(parameters.Cast<BslValue>());

            return _callable.Invoke(obj, bslArguments.ToArray());
        }
        
    }
}