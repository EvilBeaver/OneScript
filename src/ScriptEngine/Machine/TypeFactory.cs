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
using ScriptEngine.Types;
using Refl = System.Reflection;

namespace ScriptEngine.Machine
{
    public delegate IValue InstanceConstructor(TypeActivationContext context, IValue[] arguments);
    
    public class TypeFactory
    {
        private readonly TypeDescriptor _systemType;

        private Dictionary<int, InstanceConstructor> _constructorsCache = new Dictionary<int, InstanceConstructor>();

        public TypeFactory(TypeDescriptor type)
        {
            _systemType = type;
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

            var typeCast = typeof(ContextValuesMarshaller).GetMethod("ConvertParam", new[]{typeof(IValue),typeof(Type)});
            System.Diagnostics.Debug.Assert(typeCast != null);

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
                    var conversionArg = Expression.ArrayIndex(argsParam,
                                                              Expression.Constant(i)
                                                              );
                    var marshalledArg = Expression.Call(typeCast, conversionArg, Expression.Constant(parameters[paramIndex].ParameterType));
                    argsToPass.Add(
                        Expression.Convert(marshalledArg, parameters[paramIndex].ParameterType)
                        );
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

#pragma warning disable 618
                if (attribute.ParametrizeWithClassName)
                {
                    if(parameters.Length == 0 || parameters[0].ParameterType != typeof(string))
                        throw new InvalidOperationException("Type parametrized constructor must have first argument of type String");
                }

                var definition = new ConstructorDefinition
                {
                    CtorInfo = method,
                    Parametrized = attribute.ParametrizeWithClassName || injectContext,
                    InjectContext = injectContext
                };
#pragma warning restore 618
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
