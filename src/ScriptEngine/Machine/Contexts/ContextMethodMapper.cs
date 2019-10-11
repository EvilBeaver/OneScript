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

        private void Init()
        {
            if (_methodPtrs == null)
            {
                lock (this)
                {
                    if (_methodPtrs == null)
                    {
                        _methodPtrs = new List<InternalMethInfo>();
                        MapType(typeof(TInstance));
                    }
                }
            }
        }

        private void InitSearch()
        {
            if (_methodNumbers == null)
            {
                Init();
                _methodNumbers = new IdentifiersTrie<int>();
                for (int idx = 0; idx < _methodPtrs.Count; ++idx)
                {
                    var methinfo = _methodPtrs[idx].MethodInfo;

                    _methodNumbers.Add(methinfo.Name, idx);
                    if (methinfo.Alias != null)
                        _methodNumbers.Add(methinfo.Alias, idx);
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
            InitSearch();

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

        private void MapType(Type type)
        {
            _methodPtrs = type.GetMethods()
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
                    var pd = new ParameterDefinition();
                    if (parameters[i].GetCustomAttributes(typeof(ByRefAttribute), false).Length != 0)
                    {
                        if (parameters[i].ParameterType != typeof(IVariable))
                        {
                            throw new InvalidOperationException("Attribute ByRef can be applied only on IVariable parameters");
                        }
                        pd.IsByValue = false;
                    }
                    else
                    {
                        pd.IsByValue = true;
                    }

                    if (parameters[i].IsOptional)
                    {
                        pd.HasDefaultValue = true;
                        pd.DefaultValueIndex = ParameterDefinition.UNDEFINED_VALUE_INDEX;
                    }

                    paramDefs[i] = pd;

                }

                var scriptMethInfo = new ScriptEngine.Machine.MethodInfo();
                scriptMethInfo.IsFunction = isFunc;
                scriptMethInfo.IsExport = true;
                scriptMethInfo.IsDeprecated = binding.IsDeprecated;
                scriptMethInfo.ThrowOnUseDeprecated = binding.ThrowOnUse;
                scriptMethInfo.Name = binding.GetName();
                scriptMethInfo.Alias = binding.GetAlias(target.Name);

                scriptMethInfo.Params = paramDefs;

                return scriptMethInfo;
            }

            private static ContextCallableDelegate<TInstance> CreateFunction(System.Reflection.MethodInfo target)
            {
                var methodCall = MethodCallExpression(target, out var instParam, out var argsParam);

                var convertRetMethod = typeof(InternalMethInfo).GetMethod("ConvertReturnValue", BindingFlags.Static | BindingFlags.NonPublic)?.MakeGenericMethod(target.ReturnType);
                System.Diagnostics.Debug.Assert(convertRetMethod != null);
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
                    var convertMethod = typeof(InternalMethInfo).GetMethod("ConvertParam",
                                            BindingFlags.Static | BindingFlags.NonPublic,
                                            null,
                                            new Type[]
                                            {
                                                typeof(IValue),
                                                typeof(object)
                                            },
                                            null)?.MakeGenericMethod(parameters[i].ParameterType);
                    System.Diagnostics.Debug.Assert(convertMethod != null);

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

            private static T ConvertParam<T>(IValue value)
            {
                return ContextValuesMarshaller.ConvertParam<T>(value);
            }

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
