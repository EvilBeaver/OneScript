/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Language;

namespace ScriptEngine.Machine.Contexts
{
    public delegate IValue ContextCallableDelegate<TInstance>(TInstance instance, IValue[] args, IBslProcess process);

    public class ContextMethodsMapper<TInstance>
    {
        private List<InternalMethInfo> _methodPtrs;
        private IdentifiersTrie<int> _methodNumbers;

        private readonly object _locker = new object();

        private void Init()
        {
            if (_methodPtrs == null)
            {
                lock (_locker)
                {
                    if (_methodPtrs == null)
                    {
                        var localPtrs = MapType(typeof(TInstance));
                        _methodNumbers = new IdentifiersTrie<int>();
                        for (int idx = 0; idx < localPtrs.Count; ++idx)
                        {
                            var methinfo = localPtrs[idx].MethodSignature;

                            _methodNumbers.Add(methinfo.Name, idx);
                            if (methinfo.Alias != null)
                                _methodNumbers.Add(methinfo.Alias, idx);
                        }

                        _methodPtrs = localPtrs;
                    }
                }
            }
        }

        public ContextCallableDelegate<TInstance> GetCallableDelegate(int number)
        {
            Init();
            return _methodPtrs[number].Method;
        }

        public BslMethodInfo GetRuntimeMethod(int number)
        {
            Init();
            return _methodPtrs[number].ClrMethod;
        }

        public IEnumerable<BslMethodInfo> GetMethods()
        {
            Init();
            return _methodPtrs.Select(x => x.ClrMethod);
        }

        public int FindMethod(string name)
        {
            Init();

            if (!_methodNumbers.TryGetValue(name, out var idx))
                throw RuntimeException.MethodNotFoundException(name);

            return idx;
        }

        public int Count
        {
            get
            {
                Init();
                return _methodPtrs.Count;
            }
        }

        private static List<InternalMethInfo> MapType(Type type)
        {
            var mappedMethods = new List<InternalMethInfo>();
            foreach (var methodInfo in type.GetMethods()
                         .Where(method => Attribute.IsDefined(method, typeof(ContextMethodAttribute))))
            {
                var mainMarkup = methodInfo.GetCustomAttribute<ContextMethodAttribute>();
                var mainMapping = new InternalMethInfo(methodInfo, mainMarkup);
                mappedMethods.Add(mainMapping);
                mappedMethods.AddRange(methodInfo.GetCustomAttributes<DeprecatedNameAttribute>()
                    .Select(deprecation => new InternalMethInfo(methodInfo, new ContextMethodAttribute(deprecation.Name, default)
                    {
                        IsDeprecated = true,
                        ThrowOnUse = deprecation.ThrowOnUse
                    }))
                );
            }

            return mappedMethods;
        }

        private class InternalMethInfo
        {
            private readonly Lazy<ContextCallableDelegate<TInstance>> _method;
            private readonly ContextMethodInfo _clrMethod;
            
            public MethodSignature MethodSignature { get; }
            public BslMethodInfo ClrMethod => _clrMethod;

            public InternalMethInfo(MethodInfo target, ContextMethodAttribute binding)
            {
                _clrMethod = new ContextMethodInfo(target, binding);
                MethodSignature = CreateMetadata(target, binding, _clrMethod.InjectsProcess);
                
                _method = new Lazy<ContextCallableDelegate<TInstance>>(() =>
                {
                    var isFunc = target.ReturnType != typeof(void);
                    return isFunc ? CreateFunction(_clrMethod) : CreateProcedure(_clrMethod);
                });
            }

            public ContextCallableDelegate<TInstance> Method => _method.Value;

            private static MethodSignature CreateMetadata(MethodInfo target, ContextMethodAttribute binding,
                bool hasProcessParam)
            {
                return CreateMetadata(target, binding.Name, binding.Alias, binding.IsDeprecated, binding.ThrowOnUse,
                    hasProcessParam);
            }
            
            private static MethodSignature CreateMetadata(
                MethodInfo target,
                string name,
                string alias,
                bool isDeprecated,
                bool throwOnUse,
                bool hasProcessParam)
            {
                var parameters = target.GetParameters();
                var isFunc = target.ReturnType != typeof(void);
                
                var (startIndex, argNum) = hasProcessParam ? (1, parameters.Length - 1) : (0, parameters.Length);

                var paramDefs = new ParameterDefinition[argNum];
                for (int i = 0, j = startIndex; i < argNum; i++, j++)
                {
                    var pd = new ParameterDefinition();
                    if (parameters[j].GetCustomAttributes(typeof(ByRefAttribute), false).Length != 0)
                    {
                        if (parameters[j].ParameterType != typeof(IVariable))
                        {
                            throw new InvalidOperationException("Attribute ByRef can be applied only on IVariable parameters");
                        }
                        pd.IsByValue = false;
                    }
                    else
                    {
                        pd.IsByValue = true;
                    }

                    if (parameters[j].IsOptional)
                    {
                        pd.HasDefaultValue = true;
                        pd.DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX;
                    }

                    paramDefs[i] = pd;

                }

                return new MethodSignature
                {
                    IsFunction = isFunc,
                    IsExport = true,
                    IsDeprecated = isDeprecated,
                    ThrowOnUseDeprecated = throwOnUse,
                    Name = name,
                    Alias = alias,
                    Params = paramDefs
                };
            }

            private static ContextCallableDelegate<TInstance> CreateFunction(ContextMethodInfo target)
            {
                var methodCall = MethodCallExpression(target, out var instParam, out var argsParam, out var processParam);

                var convertRetMethod = ContextValuesMarshaller.BslReturnValueGenericConverter.MakeGenericMethod(target.ReturnType);
                var convertReturnCall = Expression.Call(convertRetMethod, methodCall);
                var body = convertReturnCall;

                var l = Expression.Lambda<ContextCallableDelegate<TInstance>>(body, instParam, argsParam, processParam);

                return l.Compile();

            }
            private static ContextCallableDelegate<TInstance> CreateProcedure(ContextMethodInfo target)
            {
                var methodCall = MethodCallExpression(target, out var instParam, out var argsParam, out var processParam);
                var returnLabel = Expression.Label(typeof(IValue));
                var defaultValue = Expression.Constant(null, typeof(IValue));
                var returnExpr = Expression.Return(
                    returnLabel,
                    defaultValue,
                    typeof(IValue)
                );

                var body = Expression.Block(
                    methodCall,
                    returnExpr,
                    Expression.Label(returnLabel, defaultValue)
                    );

                var l = Expression.Lambda<ContextCallableDelegate<TInstance>>(body, instParam, argsParam, processParam);
                return l.Compile();
            }

            private static InvocationExpression MethodCallExpression(
                ContextMethodInfo contextMethod, 
                out ParameterExpression instParam,
                out ParameterExpression argsParam,
                out ParameterExpression processParam)
            {
                // For those who dare:
                // Код ниже формирует следующую лямбду с 2-мя замыканиями realMethodDelegate и defaults:
                // (inst, args) =>
                // {
                //    realMethodDelegate(inst,
                //        ConvertParam<TypeOfArg1>(args[i], defaults[i]),
                //        ...
                //        ConvertParam<TypeOfArgN>(args[i], defaults[i]));
                // }

                var target = contextMethod.GetWrappedMethod();
                var methodClojure = CreateDelegateExpr(target);

                instParam = Expression.Parameter(typeof(TInstance), "inst");
                argsParam = Expression.Parameter(typeof(IValue[]), "args");
                processParam = Expression.Parameter(typeof(IBslProcess), "process");

                var parameters = target.GetParameters();

                var (clrIndexStart, argsLen) = contextMethod.InjectsProcess ? (1, parameters.Length - 1) : (0, parameters.Length);
                
                var argsPass = new List<Expression>();
                argsPass.Add(instParam);
                
                if (contextMethod.InjectsProcess)
                    argsPass.Add(processParam);
                
                for (int bslIndex = 0,clrIndex = clrIndexStart; bslIndex < argsLen; bslIndex++, clrIndex++)
                {
                    var targetType = parameters[clrIndex].ParameterType;
                    var convertMethod = ContextValuesMarshaller.BslGenericParameterConverter.MakeGenericMethod(targetType);
                    
                    Expression defaultArg;
                    if (parameters[clrIndex].HasDefaultValue)
                    {
                        defaultArg = Expression.Constant(parameters[clrIndex].DefaultValue, targetType);
                    }
                    else
                    {
                        defaultArg = ContextValuesMarshaller.GetDefaultBslValueConstant(targetType);
                    }

                    var indexedArg = Expression.ArrayIndex(argsParam, Expression.Constant(bslIndex));
                    var conversionCall = Expression.Call(convertMethod,
                        indexedArg,
                        defaultArg,
                        processParam);
                    
                    argsPass.Add(Expression.Convert(conversionCall, targetType));
                }

                var methodCall = Expression.Invoke(methodClojure, argsPass);
                return methodCall;
            }

            private static Expression CreateDelegateExpr(MethodInfo target)
            {
                var types = new List<Type>();
                types.Add(target.DeclaringType);
                types.AddRange(target.GetParameters().Select(x => x.ParameterType));
                Type delegateType;
                if (target.ReturnType == typeof(void))
                {
                    delegateType = Expression.GetActionType(types.ToArray());
                }
                else
                {
                    types.Add(target.ReturnType);
                    delegateType = Expression.GetFuncType(types.ToArray());
                }

                var deleg = target.CreateDelegate(delegateType);

                var delegateExpr = Expression.Constant(deleg);
                var conversion = Expression.Convert(delegateExpr, delegateType);

                var delegateCreator = Expression.Lambda(conversion).Compile();
                var methodClojure = Expression.Constant(delegateCreator.DynamicInvoke());

                return methodClojure;
            }
        }
    }
}
