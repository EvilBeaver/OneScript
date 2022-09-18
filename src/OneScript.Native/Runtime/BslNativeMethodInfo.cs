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
        private Lazy<CallableMethod> _callable;
        
        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
            _callable = new Lazy<CallableMethod>(CreateCallable, LazyThreadSafetyMode.PublicationOnly);
        }

        private CallableMethod CreateCallable()
        {
            return new CallableMethod(this);
        }

        internal CallableMethod GetCallable() => _callable.Value;

        public LambdaExpression Implementation { get; private set; }

        public bool IsInstance { get; internal set; }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            var bslArguments = new List<BslValue>(parameters.Length);
            bslArguments.AddRange(parameters.Cast<BslValue>());

            return _callable.Value.Invoke(obj, bslArguments.ToArray());
        }
        
    }
}