/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace OneScript.DebugProtocol
{
    public class MethodsDispatcher<TInterface>
    {
        private delegate object ProtocolMethod(TInterface target, object[] parameters);
        private readonly Dictionary<string, ProtocolMethod> _methods = new Dictionary<string, ProtocolMethod>();

        public MethodsDispatcher()
        {
            var type = typeof(TInterface);
            if (!type.IsInterface)
            {
                throw new ArgumentException("Type must be an interface");
            }

            foreach (var method in type.GetMethods())
            {
                AddMethod(method);
            }
        }

        private void AddMethod(MethodInfo method)
        {
            var targetParam = Expression.Parameter(typeof(TInterface));
            var argumentsParam = Expression.Parameter(typeof(object[]));

            var parameters = method.GetParameters();
            var castedArguments = new Expression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var index = Expression.Constant(i);
                var accessToIndexOfArg = Expression.ArrayIndex(argumentsParam, index);
                var castExpression = Expression.Convert(accessToIndexOfArg, parameter.ParameterType);
                castedArguments[i] = castExpression;
            }

            Expression body;
            var call = Expression.Call(targetParam, method, castedArguments);
            if (method.ReturnType == typeof(void))
            {
                var methodEnd = Expression.Label(typeof(object));
                var nullConstant = Expression.Constant(null, typeof(object));
                var returnNewObject = Expression.Return(methodEnd, nullConstant);
                body = Expression.Block(call, returnNewObject, Expression.Label(methodEnd, nullConstant));
            }
            else
            {
                body = call;
            }


            var lambda = Expression.Lambda<ProtocolMethod>(body, targetParam, argumentsParam);
            _methods.Add(method.Name, lambda.Compile());
        }

        public object Dispatch(TInterface target, string methodName, object[] args)
        {
            var method = _methods[methodName];
            return method(target, args);
        }
    }
}