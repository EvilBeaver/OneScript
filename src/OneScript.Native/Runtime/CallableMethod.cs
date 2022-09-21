/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OneScript.Contexts;
using OneScript.Native.Compiler;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    /// <summary>
    /// Класс-обертка для вызываемой лямбды. Нужен для обхода проблемы
    /// MethodInfo must be a runtime MethodInfo
    /// Получаемой выражением Expression.Constant(bslNativeMethodInfo)
    ///
    /// Иными словами, не удается составить выражение, вызывающее кастомный метод в кастомном наследнике MethodInfo
    /// Это похоже на баг в .NET, а может это byDesign, так или иначе, метод вызова отделен от BslNativeMethodInfo
    /// для использования в выражениях Call (<see cref="ExpressionHelpers.InvokeBslNativeMethod"/>)
    /// </summary>
    internal class CallableMethod
    {
        private readonly BslNativeMethodInfo _method;

        private delegate BslValue NativeCallable(NativeClassInstanceWrapper target, BslValue[] args);

        private readonly NativeCallable _delegate;
        
        public CallableMethod(BslNativeMethodInfo method)
        {
            _method = method;
            _delegate = CreateDelegate(method);
        }

        public BslValue Invoke(object target, BslValue[] args)
        {
            var callableWrapper = GetCallableWrapper(target);
            return _delegate.Invoke(callableWrapper, args);
        }
        
        private NativeClassInstanceWrapper GetCallableWrapper(object obj)
        {
            NativeClassInstanceWrapper callableWrapper;
            if (_method.IsInstance)
            {
                if (obj == null)
                    throw new InvalidOperationException($"Method {_method.Name} is not static and requires target");
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
                    throw new InvalidOperationException($"Method {_method.Name} is static");

                callableWrapper = null;
            }

            return callableWrapper;
        }

        private static NativeCallable CreateDelegate(BslNativeMethodInfo method)
        {
            if (method.Implementation == default)
                throw new InvalidOperationException("Method has no implementation");

            var targetParam = Expression.Parameter(typeof(NativeClassInstanceWrapper));
            var arrayOfValuesParam = Expression.Parameter(typeof(BslValue[]));
            var convertedAccessList = new List<Expression>();

            if (method.IsInstance)
                convertedAccessList.Add(targetParam);
            
            int index = 0;
            foreach (var parameter in method.GetBslParameters())
            {
                var targetType = parameter.ParameterType;
                var arrayAccess = Expression.ArrayIndex(arrayOfValuesParam, Expression.Constant(index));
                var convertedParam = ExpressionHelpers.ConvertToType(arrayAccess, targetType);
                convertedAccessList.Add(convertedParam);
                ++index;
            }
            
            var lambdaInvocation = Expression.Invoke(method.Implementation, convertedAccessList);
            var func = Expression.Lambda<NativeCallable>(lambdaInvocation, targetParam, arrayOfValuesParam);

            return func.Compile();
        }
    }
}
