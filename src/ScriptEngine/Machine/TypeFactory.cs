/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Refl = System.Reflection;

namespace ScriptEngine.Machine
{
    public delegate IValue InstanceConstructor(string typeName, IValue[] arguments);

    public class TypeFactory
    {
        private readonly Type _clrType;

        private Dictionary<int, InstanceConstructor> _constructorsCache = new Dictionary<int, InstanceConstructor>();

        static private readonly Refl.MethodInfo _typeCast =
            typeof(ContextValuesMarshaller).GetMethods()
            .First(x => x.Name == "ConvertParam" && x.GetGenericArguments().Length == 0);

        static private readonly Refl.MethodInfo _genTypeCast =
            typeof(ContextValuesMarshaller).GetMethods()
            .First(x => x.Name == "ConvertParam" && x.GetGenericArguments().Length == 1);


        public TypeFactory(Type clrType)
        {
            System.Diagnostics.Debug.Assert(_typeCast != null);
            System.Diagnostics.Debug.Assert(_genTypeCast != null);

            _clrType = clrType;
        }

        public InstanceConstructor GetConstructor(string typeName, IValue[] arguments)
        {
            if (_constructorsCache.TryGetValue(arguments.Length, out var constructor))
            {
                return constructor;
            }

            constructor = CreateConstructor(typeName, arguments);
            if(constructor != null)
                _constructorsCache[arguments.Length] = constructor;

            return constructor;
        }

        private InstanceConstructor CreateConstructor(string typeName, IValue[] arguments)
        {
            var definition = FindConstructor(arguments);
            if (definition == null)
                return null;

            var methodInfo = definition.Value.CtorInfo;
            if (!typeof(IValue).IsAssignableFrom(methodInfo.ReturnType))
            {
                return FallbackConstructor(methodInfo);
            }

            var argsParam = Expression.Parameter(typeof(IValue[]), "args");
            var parameters = methodInfo.GetParameters();
            var argsToPass = new List<Expression>();
            var typeNameParam = Expression.Parameter(typeof(string), "typeName");

            int paramIndex = 0;
            if (definition.Value.Parametrized && parameters.Length > 0)
            {
                argsToPass.Add(typeNameParam);
                ++paramIndex;
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (parameters[paramIndex].ParameterType.IsArray)
                {
                    // capture all

                    var copyMethod = typeof(TypeFactory).GetMethod("CaptureVariantArgs", Refl.BindingFlags.Static | Refl.BindingFlags.InvokeMethod | Refl.BindingFlags.NonPublic);
                    System.Diagnostics.Debug.Assert(copyMethod != null);

                    argsToPass.Add(Expression.Call(copyMethod, argsParam, Expression.Constant(i)));
                    ++paramIndex;
                    break;
                }

                
                if (parameters[paramIndex].ParameterType == typeof(IValue))
                    argsToPass.Add(Expression.ArrayIndex(argsParam, Expression.Constant(i)));
                else
                {
                    if (parameters[i].HasDefaultValue && 
                        (arguments[i] == null || arguments[i].DataType == DataType.NotAValidValue) )
                    {
                        argsToPass.Add(Expression.Convert(Expression.Constant(parameters[paramIndex].DefaultValue), parameters[paramIndex].ParameterType));
                    }
                    else
                    {
                        var conversionArg = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                        var marshalledArg = Expression.Call(_typeCast, conversionArg, Expression.Constant(parameters[paramIndex].ParameterType));
                        argsToPass.Add(Expression.Convert(marshalledArg, parameters[paramIndex].ParameterType));
                    }
                }

                ++paramIndex;
            }

            for (int i = paramIndex; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType.IsArray)
                {
                    argsToPass.Add(Expression.NewArrayInit(typeof(IValue)));
                }
                else
                {
                    argsToPass.Add(Expression.Convert(Expression.Constant(parameters[i].DefaultValue), parameters[i].ParameterType));
                }
            }

            var constructorCallExpression = Expression.Call(methodInfo, argsToPass);
            var callLambda = Expression.Lambda<InstanceConstructor>(constructorCallExpression, typeNameParam, argsParam).Compile();

            return callLambda;
        }

        // Конструктор использующий старое поведение.
        // применяется для старых внешних библиотек, в которых StaticConstructor возвращает
        // не IValue а IRuntimeContextInstance.
        //
        private InstanceConstructor FallbackConstructor(Refl.MethodInfo methodInfo)
        {
            return (typeName, args) =>
            {
                var methArgs = new IValue[methodInfo.GetParameters().Length];
                for (int i = 0; i < methArgs.Length; i++)
                {
                    if (i < args.Length)
                        methArgs[i] = args[i];
                }
                return (IValue) methodInfo.Invoke(null, methArgs);
            };
        }

        // ReSharper disable once UnusedMember.Global
        internal static IValue[] CaptureVariantArgs(IValue[] sourceArgs, int startingFrom)
        {
            var newArray = new IValue[sourceArgs.Length - startingFrom];
            Array.Copy(sourceArgs, startingFrom, newArray, 0, newArray.Length);
            return newArray;
        }

        private ConstructorDefinition? FindConstructor(IValue[] arguments)
        {
            var ctors = _clrType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                            .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute))
                            .Select(x => new ConstructorDefinition
                            {
                                CtorInfo = x,
                                Parametrized = ((ScriptConstructorAttribute)x.GetCustomAttributes(typeof(ScriptConstructorAttribute), false)[0]).ParametrizeWithClassName
                            });


            int argCount = arguments.Length;
            foreach (var ctor in ctors)
            {
                var parameters = ctor.CtorInfo.GetParameters();

                if (ctor.Parametrized && parameters.Length > 0)
                {
                    if (parameters[0].ParameterType != typeof(string))
                    {
                        throw new InvalidOperationException("Type parametrized constructor must have first argument of type String");
                    }

                    parameters = parameters.Skip(1).ToArray();
                }

                bool success = (parameters.Length == 0 && argCount == 0)
                    || (parameters.Length > 0 && parameters[0].ParameterType.IsArray);

                if (success)
                    return ctor;

                if (parameters.Length > 0 && parameters.Length < argCount
                    && !parameters[parameters.Length - 1].ParameterType.IsArray)
                {
                    success = false;
                    continue;
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsArray)
                    {
                        // captures all remained args
                        success = true;
                        break;
                    }
                    else
                    {
                        if (i < argCount)
                        {
                            success = true;
                        }
                        else
                        {
                            if (parameters[i].IsOptional)
                            {
                                success = true;
                            }
                            else
                            {
                                success = false;
                                break; // no match
                            }
                        }
                    }
                }

                if (success)
                    return ctor;
            }

            return null;
        }

        private struct ConstructorDefinition
        {
            public Refl.MethodInfo CtorInfo { get; set; }
            public bool Parametrized { get; set; }
        }
    }
}
