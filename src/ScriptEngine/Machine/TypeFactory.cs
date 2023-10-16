﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using OneScript.Types;
using OneScript.Contexts;
using OneScript.Exceptions;
using ScriptEngine.Types;
using Refl = System.Reflection;

namespace ScriptEngine.Machine
{
    internal delegate IValue InstanceConstructor(TypeActivationContext context, IValue[] arguments);
    
    public class TypeFactory : ITypeFactory
    {
        private readonly TypeDescriptor _systemType;

        private Dictionary<int, InstanceConstructor> _constructorsCache = new Dictionary<int, InstanceConstructor>();

        private static readonly Refl.MethodInfo _typeCast =
            typeof(ContextValuesMarshaller).GetMethods()
            .First(x => x.Name == "ConvertParam" && x.GetGenericArguments().Length == 0);

        private static readonly Refl.MethodInfo _genTypeCast =
            typeof(ContextValuesMarshaller).GetMethods()
            .First(x => x.Name == "ConvertParamDef" && x.GetGenericArguments().Length == 1);
        
        public TypeFactory(TypeDescriptor type)
        {
            System.Diagnostics.Debug.Assert(_typeCast != null);
            System.Diagnostics.Debug.Assert(_genTypeCast != null);
            
            _systemType = type;
        }
        
        public TypeFactory(Type type) : this(type.GetTypeFromClassMarkup())
        {
        }

        private Type ClrType => _systemType.ImplementingClass;

        public IValue Activate(TypeActivationContext context, IValue[] arguments)
        {
            var constructor = GetConstructor(arguments);
            if (constructor == default)
            {
                throw RuntimeException.ConstructorNotFound(context.TypeName);
            }
            
            var instance = constructor(context, arguments);
            if (instance is ISystemTypeAcceptor typeAcceptor)
            {
                typeAcceptor.AssignType(_systemType);
            }

            return instance;
        }
        
        private InstanceConstructor GetConstructor(IValue[] arguments)
        {
            if (_constructorsCache.TryGetValue(arguments.Length, out var constructor))
            {
                return constructor;
            }

            constructor = CreateConstructor(arguments);
            if(constructor != null)
                _constructorsCache[arguments.Length] = constructor;

            return constructor;
        }

        private InstanceConstructor CreateConstructor(IValue[] arguments)
        {
            var (success, definition) = FindConstructor(arguments);
            if (!success)
                return null;

            var methodInfo = definition.CtorInfo;
            if (!typeof(IValue).IsAssignableFrom(methodInfo.ReturnType))
            {
                return FallbackConstructor(methodInfo);
            }

            var argsParam = Expression.Parameter(typeof(IValue[]), "args");
            var parameters = methodInfo.GetParameters();
            var argsToPass = new List<Expression>();
            var contextParam = Expression.Parameter(typeof(TypeActivationContext), "context");

            int paramIndex = 0;
            if (definition.Parametrized && parameters.Length > 0)
            {
                if (definition.InjectContext)
                {
                    argsToPass.Add(contextParam); 
                }
                else
                {
                    var propAccess = Expression.PropertyOrField(contextParam, nameof(TypeActivationContext.TypeName));
                    argsToPass.Add(propAccess);
                }   
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
                    var conversionArg = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    if (parameters[i].HasDefaultValue)
                    {
                        var convertMethod = _genTypeCast.MakeGenericMethod(parameters[i].ParameterType);
                        var defaultArg = Expression.Constant(parameters[i].DefaultValue);

                        var marshalledArg = Expression.Call(convertMethod, conversionArg, defaultArg);
                        argsToPass.Add(marshalledArg);
                    }
                    else
                    {
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
            var callLambda = Expression.Lambda<InstanceConstructor>(constructorCallExpression, contextParam, argsParam).Compile();

            return callLambda;
        }

        // Конструктор использующий старое поведение.
        // применяется для старых внешних библиотек, в которых StaticConstructor возвращает
        // не IValue а IRuntimeContextInstance.
        //
        private InstanceConstructor FallbackConstructor(Refl.MethodInfo methodInfo)
        {
            return (context, args) =>
            {
                var methArgs = new IValue[methodInfo.GetParameters().Length];
                for (int i = 0; i < methArgs.Length; i++)
                {
                    if (i < args.Length)
                        methArgs[i] = args[i];
                }

                try
                {
                    return (IValue) methodInfo.Invoke(null, methArgs);
                }
                catch (Refl.TargetInvocationException e)
                {
                    Debug.Assert(e.InnerException != null, "e.InnerException != null");
                    throw e.InnerException;
                }
            };
        }

        // ReSharper disable once UnusedMember.Global
        internal static IValue[] CaptureVariantArgs(IValue[] sourceArgs, int startingFrom)
        {
            var newArray = new IValue[sourceArgs.Length - startingFrom];
            Array.Copy(sourceArgs, startingFrom, newArray, 0, newArray.Length);
            return newArray;
        }

        private (bool, ConstructorDefinition) FindConstructor(IValue[] arguments)
        {
            var ctors = GetMarkedConstructors(ClrType);

            int argCount = arguments.Length;
            foreach (var ctor in ctors)
            {
                var parameters = ctor.CtorInfo.GetParameters();

                if (ctor.Parametrized)
                {
                    parameters = parameters.Skip(1).ToArray();
                }

                bool success = (parameters.Length == 0 && argCount == 0)
                    || (parameters.Length > 0 && parameters[0].ParameterType.IsArray);

                if (success)
                    return (true, ctor);

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
                    return (true, ctor);
            }

            return (false, default);
        }

        private IEnumerable<ConstructorDefinition> GetMarkedConstructors(Type type)
        {
            var staticMethods = ClrType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var constructors = new List<ConstructorDefinition>(4);
            
            foreach (var method in staticMethods)
            {
                var attribute = (ScriptConstructorAttribute) method.GetCustomAttributes(false)
                    .FirstOrDefault(y => y is ScriptConstructorAttribute);
                if(attribute == default)
                    continue;
                
                var parameters = method.GetParameters();
                var injectContext = parameters.Length > 0 &&
                                    parameters[0].ParameterType == typeof(TypeActivationContext);

                var definition = new ConstructorDefinition
                {
                    CtorInfo = method,
                    Parametrized = injectContext,
                    InjectContext = injectContext
                };

                constructors.Add(definition);
            }
            
            return constructors;
        }
        
        private struct ConstructorDefinition
        {
            public Refl.MethodInfo CtorInfo { get; set; }
            public bool Parametrized { get; set; }
            
            public bool InjectContext { get; set; }
        }
    }
}
