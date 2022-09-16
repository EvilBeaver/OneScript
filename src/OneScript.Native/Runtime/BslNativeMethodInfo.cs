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
        private delegate BslValue NativeCallable(NativeClassInstanceWrapper target, BslValue[] args);
        
        private Lazy<NativeCallable> _delegate;

        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
            _delegate = new Lazy<NativeCallable>(CreateDelegate, LazyThreadSafetyMode.PublicationOnly);
        }

        public LambdaExpression Implementation { get; private set; }

        public bool IsInstance { get; internal set; }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            var callableWrapper = GetCallableWrapper(obj);

            var bslArguments = new List<BslValue>(parameters.Length);
            bslArguments.AddRange(parameters.Cast<BslValue>());

            return _delegate.Value.Invoke(callableWrapper, bslArguments.ToArray());
        }

        internal BslValue InvokeInternal(object target, BslValue[] args)
        {
            var callableWrapper = GetCallableWrapper(target);
            return _delegate.Value.Invoke(callableWrapper, args);
        }

        private NativeClassInstanceWrapper GetCallableWrapper(object obj)
        {
            NativeClassInstanceWrapper callableWrapper;
            if (IsInstance)
            {
                if (obj == null)
                    throw new InvalidOperationException($"Method {Name} is not static and requires target");
                if (obj is NativeClassInstanceWrapper w)
                {
                    callableWrapper = w;
                }
                else if (obj is IAttachableContext context)
                {
                    context.OnAttach(out var state, out var methods);

                    callableWrapper = new NativeClassInstanceWrapper
                    {
                        Context = context,
                        State = state,
                        Methods = methods
                    };
                }
                else
                {
                    throw new ArgumentException($"Invalid argument type {obj.GetType()}", nameof(obj));
                }
            }
            else
            {
                if (obj != null)
                    throw new InvalidOperationException($"Method {Name} is static");

                callableWrapper = null;
            }

            return callableWrapper;
        }

        private NativeCallable CreateDelegate()
        {
            if (Implementation == default)
                throw new InvalidOperationException("Method has no implementation");

            var targetParam = Expression.Parameter(typeof(NativeClassInstanceWrapper));
            var arrayOfValuesParam = Expression.Parameter(typeof(BslValue[]));
            var convertedAccessList = new List<Expression>();

            if (IsInstance)
                convertedAccessList.Add(targetParam);
            
            int index = 0;
            foreach (var parameter in GetBslParameters())
            {
                var targetType = parameter.ParameterType;
                var arrayAccess = Expression.ArrayIndex(arrayOfValuesParam, Expression.Constant(index));
                var convertedParam = ExpressionHelpers.ConvertToType(arrayAccess, targetType);
                convertedAccessList.Add(convertedParam);
                ++index;
            }
            
            var lambdaInvocation = Expression.Invoke(Implementation, convertedAccessList);
            var func = Expression.Lambda<NativeCallable>(lambdaInvocation, targetParam, arrayOfValuesParam);

            return func.Compile();
        }
    }
}