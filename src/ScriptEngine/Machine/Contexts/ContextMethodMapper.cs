﻿/*----------------------------------------------------------
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
using OneScript.Language;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ContextMethodAttribute : Attribute
    {
        private readonly string _name;
        private readonly string _alias;

        public ContextMethodAttribute(string name, string alias = null)
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            _name = name;
            _alias = alias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }

        public string GetAlias(string nativeMethodName)
        {
            if (!string.IsNullOrEmpty(_alias))
            {
                return _alias;
            }
            if (!IsDeprecated)
            {
                return nativeMethodName;
            }
            return null;
        }

        public bool IsDeprecated { get; set; }

        public bool ThrowOnUse { get; set; }

        public bool IsFunction { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ByRefAttribute : Attribute
    {
    }

    public delegate IValue ContextCallableDelegate<TInstance>(TInstance instance, IValue[] args);

    public class ContextMethodsMapper<TInstance>
    {
        private List<InternalMethInfo> _methodPtrs;
        private IdentifiersTrie<int> _methodNumbers;

        private readonly object _locker = new object();

        static private readonly System.Reflection.MethodInfo _genConvertParamMethod =
            typeof(InternalMethInfo).GetMethod("ConvertParam",
            BindingFlags.Static | BindingFlags.NonPublic);

        static private readonly System.Reflection.MethodInfo _genConvertReturnMethod =
            typeof(InternalMethInfo).GetMethod("ConvertReturnValue",
            BindingFlags.Static | BindingFlags.NonPublic);


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
                            var methinfo = localPtrs[idx].MethodInfo;

                            _methodNumbers.Add(methinfo.Name, idx);
                            if (methinfo.Alias != null)
                                _methodNumbers.Add(methinfo.Alias, idx);
                        }

                        _methodPtrs = localPtrs;
                    }
                }
            }
        }

        public ContextCallableDelegate<TInstance> GetMethod(int number)
        {
            Init();
            return _methodPtrs[number].Method;
        }

        public ScriptEngine.Machine.MethodInfo GetMethodInfo(int number)
        {
            Init();
            return _methodPtrs[number].MethodInfo;
        }

        public IEnumerable<MethodInfo> GetMethods()
        {
            Init();
            return _methodPtrs.Select(x => x.MethodInfo);
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

        private List<InternalMethInfo> MapType(Type type)
        {
            return type.GetMethods()
                .SelectMany(method => method.GetCustomAttributes(typeof(ContextMethodAttribute), false)
                    .Select(attr => new InternalMethInfo(method, (ContextMethodAttribute)attr)) )
                .ToList();
        }

        private class InternalMethInfo
        {
            private readonly Lazy<ContextCallableDelegate<TInstance>> _method;
            public MethodInfo MethodInfo { get; }

            public InternalMethInfo(System.Reflection.MethodInfo target, ContextMethodAttribute binding)
            {
                _method = new Lazy<ContextCallableDelegate<TInstance>>(() =>
                {
                    var isFunc = target.ReturnType != typeof(void);
                    return isFunc ? CreateFunction(target) : CreateProcedure(target);
                });

                MethodInfo = CreateMetadata(target, binding);
            }

            public ContextCallableDelegate<TInstance> Method => _method.Value;

            private static MethodInfo CreateMetadata(System.Reflection.MethodInfo target, ContextMethodAttribute binding)
            {
                var parameters = target.GetParameters();
                var isFunc = target.ReturnType != typeof(void);
                var argNum = parameters.Length;

                var paramDefs = new ParameterDefinition[argNum];
                for (int i = 0; i < argNum; i++)
                {
                    var param = parameters[i];

                    var pd = new ParameterDefinition();
                    pd.RealType = param.ParameterType;

                    if (param.GetCustomAttributes(typeof(ByRefAttribute), false).Length != 0)
                    {
                        if (param.ParameterType != typeof(IVariable))
                            throw new InvalidOperationException("Attribute ByRef can be applied only on IVariable parameters");

                        pd.IsByValue = false;
                    }
                    else
                    {
                        pd.IsByValue = true;
                    }

                    if (param.HasDefaultValue)
                    {
                        pd.HasDefaultValue = true;
                        pd.DefaultValue = ContextValuesMarshaller.ConvertParameterDefaultValue(param);
                        pd.DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX;
                    }

                    paramDefs[i] = pd;
                }

                return new MethodInfo
                {
                    IsFunction = isFunc,
                    IsExport = true,
                    IsDeprecated = binding.IsDeprecated,
                    ThrowOnUseDeprecated = binding.ThrowOnUse,
                    Name = binding.GetName(),
                    Alias = binding.GetAlias(target.Name),
                    Params = paramDefs
                };
            }

            private static ContextCallableDelegate<TInstance> CreateFunction(System.Reflection.MethodInfo target)
            {
                var methodCall = MethodCallExpression(target, out var instParam, out var argsParam);

                var convertRetMethod = _genConvertReturnMethod.MakeGenericMethod(target.ReturnType);
                //System.Diagnostics.Debug.Assert(convertRetMethod != null);
                var convertReturnCall = Expression.Call(convertRetMethod, methodCall);
                var body = convertReturnCall;

                var l = Expression.Lambda<ContextCallableDelegate<TInstance>>(body, instParam, argsParam);

                return l.Compile();

            }
            private static ContextCallableDelegate<TInstance> CreateProcedure(System.Reflection.MethodInfo target)
            {
                var methodCall = MethodCallExpression(target, out var instParam, out var argsParam);
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

                var l = Expression.Lambda<ContextCallableDelegate<TInstance>>(body, instParam, argsParam);
                return l.Compile();
            }

            private static InvocationExpression MethodCallExpression(System.Reflection.MethodInfo target, out ParameterExpression instParam, out ParameterExpression argsParam)
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

                var methodClojure = CreateDelegateExpr(target);

                instParam = Expression.Parameter(typeof(TInstance), "inst");
                argsParam = Expression.Parameter(typeof(IValue[]), "args");

                var argsPass = new List<Expression>();
                argsPass.Add(instParam);

                var parameters = target.GetParameters();
                object[] defaultValues = new object[parameters.Length];
                var defaultsClojure = Expression.Constant(defaultValues);

                for (int i = 0; i < parameters.Length; i++)
                {
                    var convertMethod = _genConvertParamMethod.MakeGenericMethod(parameters[i].ParameterType);

                    if (parameters[i].HasDefaultValue)
                    {
                        defaultValues[i] = parameters[i].DefaultValue;
                    }

                    var indexedArg = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    var defaultArg = Expression.ArrayIndex(defaultsClojure, Expression.Constant(i));
                    var conversionCall = Expression.Call(convertMethod, indexedArg, defaultArg);
                    argsPass.Add(conversionCall);
                }

                var methodCall = Expression.Invoke(methodClojure, argsPass);
                return methodCall;
            }

            private static Expression CreateDelegateExpr(System.Reflection.MethodInfo target)
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

            // ReSharper disable once UnusedMember.Local
            private static T ConvertParam<T>(IValue value, object def)
            {
                if (value == null || value.DataType == DataType.NotAValidValue)
                    return (T)def;

                return ContextValuesMarshaller.ConvertParam<T>(value);
            }

            private static IValue ConvertReturnValue<TRet>(TRet param)
            {
                return ContextValuesMarshaller.ConvertReturnValue<TRet>(param);
            }
        }

    }
}
