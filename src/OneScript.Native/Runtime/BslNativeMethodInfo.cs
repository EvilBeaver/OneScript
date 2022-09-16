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
using OneScript.Native.Compiler;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    public class BslNativeMethodInfo : BslScriptMethodInfo
    {
        private Lazy<Func<BslValue[], BslValue>> _delegate;

        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
            _delegate = new Lazy<Func<BslValue[], BslValue>>(CreateDelegate, LazyThreadSafetyMode.PublicationOnly);
        }

        public LambdaExpression Implementation { get; private set; }

        public bool IsInstance { get; internal set; }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            if (!(obj is NativeClassInstanceWrapper wrapper))
                throw new ArgumentException(nameof(obj));
            
            var bslArguments = new List<BslValue>();
            bslArguments.Add((BslValue)wrapper.Context); // this
            bslArguments.AddRange(parameters.Cast<BslValue>());

            return _delegate.Value.Invoke(bslArguments.ToArray());
        }
        
        private Func<BslValue[], BslValue> CreateDelegate()
        {
            var l = Implementation;
            if (l == default)
                throw new InvalidOperationException("Method has no implementation");

            var arrayOfValuesParam = Expression.Parameter(typeof(BslValue[]));
            var convertedAccessList = new List<Expression>();

            int index = 0;
            foreach (var parameter in GetBslParameters())
            {
                var targetType = parameter.ParameterType;
                var arrayAccess = Expression.ArrayIndex(arrayOfValuesParam, Expression.Constant(index));
                var convertedParam = ExpressionHelpers.ConvertToType(arrayAccess, targetType);
                convertedAccessList.Add(convertedParam);
                ++index;
            }
            
            var lambdaInvocation = Expression.Invoke(l, convertedAccessList);
            var func = Expression.Lambda<Func<BslValue[], BslValue>>(lambdaInvocation, arrayOfValuesParam);

            return func.Compile();
        }
    }
}