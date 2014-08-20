using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Core;

namespace OneScript.ComponentModel
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ByRefAttribute : Attribute
    {
    }

    public delegate IValue ContextCallableDelegate<TInstance>(TInstance instance, IValue[] args);

    public class ContextMethodsMapper<TInstance>
    {
        private List<InternalMethInfo> _methodPtrs = new List<InternalMethInfo>();
        private Dictionary<string, int> _nameIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        
        public ContextMethodsMapper()
        {
            MapType(typeof(TInstance));
        }

        public ContextCallableDelegate<TInstance> GetMethod(int number)
        {
            return _methodPtrs[number].method;
        }

        public string GetMethodName(int index, NameRetrievalMode mode = NameRetrievalMode.Default)
        {
            var def = _methodPtrs[index].methodDef;

            switch (mode)
            {
                case NameRetrievalMode.PreferAlias:
                    if (def.Alias != "")
                        return def.Alias;
                    else
                        return def.Name;
                case NameRetrievalMode.OnlyAlias:
                    return def.Alias;
                default:
                    return def.Name;
            }

        }

        public bool HasReturnValue(int index)
        {
            return _methodPtrs[index].methodDef.IsFunction;
        }

        public int GetParametersCount(int index)
        {
            return _methodPtrs[index].methodDef.ArgCount;
        }

        public bool GetDefaultValue(int methodIndex, int paramIndex, out IValue defaultValue)
        {
            var parameters = _methodPtrs[methodIndex].methodDef.Params;
            var paramDef = parameters[paramIndex];
            if(paramDef.HasDefaultValue)
            {
                defaultValue = ValueMarshaler.CLRTypeToIValue(paramDef.DefaultValue);
                return true;
            }
            else
            {
                defaultValue = null;
                return false;
            }
        }

        public int FindMethod(string name)
        {
            name = name.ToLower();
            int idx;

            if (_nameIndexes.TryGetValue(name, out idx))
            {
                return idx;
            }
            
            throw ContextAccessException.MethodNotFound(name);
            
        }

        public int Count
        {
            get
            {
                return _methodPtrs.Count;
            }
        }

        private void MapType(Type type)
        {
            var methods = type.GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(ContextMethodAttribute), false).Any())
                .Select(x => new { 
                    Method = x, 
                    Binding = (ContextMethodAttribute)x.GetCustomAttributes(typeof(ContextMethodAttribute), false)[0] 
                });
            
            foreach (var item in methods)
            {
                const int MAX_ARG_SUPPORTED = 4;
                var parameters = item.Method.GetParameters();
                var paramTypes = parameters.Select(x=>x.ParameterType).ToList();
                var isFunc = item.Method.ReturnType != typeof(void);
                if (isFunc)
                {
                    paramTypes.Add(item.Method.ReturnType);
                }
                var argNum = paramTypes.Count;
                
                if (argNum <= MAX_ARG_SUPPORTED)
                {
                    var action = ResolveGeneric(argNum, paramTypes.ToArray(), isFunc);
                    var methPtr = (ContextCallableDelegate<TInstance>)action.Invoke(this, new object[] { item.Method });

                    if (isFunc)
                        argNum--;

                    var paramDefs = new ParameterDefinition[argNum];
                    for (int i = 0; i < argNum; i++)
                    {
                        var pd = new ParameterDefinition();
                        if (parameters[i].GetCustomAttributes(typeof(ByRefAttribute), false).Length != 0)
                        {
                            //if (paramTypes[i] != typeof(IVariable))
                            //{
                            //    throw new InvalidOperationException("Attribute ByRef can be applied only on IVariable parameters");
                            //}
                            pd.IsByValue = false;
                        }
                        else
                        {
                            pd.IsByValue = true;
                        }

                        if (parameters[i].IsOptional)
                        {
                            pd.HasDefaultValue = true;
                            pd.DefaultValue = parameters[i].DefaultValue;
                        }
                        
                        paramDefs[i] = pd;

                    }

                    var scriptMethInfo = new MethodDefinition();
                    scriptMethInfo.IsFunction = isFunc;
                    scriptMethInfo.Name = item.Binding.Name == null ? item.Method.Name : item.Binding.Name;
                    scriptMethInfo.Alias = item.Binding.Alias == null ? item.Method.Name : item.Binding.Alias;
                    scriptMethInfo.Params = paramDefs;
                    _nameIndexes.Add(scriptMethInfo.Name, _methodPtrs.Count);
                    if(scriptMethInfo.Alias != scriptMethInfo.Name)
                        _nameIndexes.Add(scriptMethInfo.Alias, _methodPtrs.Count);

                    _methodPtrs.Add(new InternalMethInfo()
                    {
                        method = methPtr,
                        methodDef = scriptMethInfo
                    });

                }
                else
                    throw new NotSupportedException(string.Format("Only {0} parameters supported", MAX_ARG_SUPPORTED));
                
            }

        }

        private System.Reflection.MethodInfo ResolveGeneric(int argNum, Type[] typeArgs, bool asFunc)
        {
            string methName = asFunc ? "CreateFunction" : "CreateAction";
            
            var method = this.GetType().GetMembers(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Where(x => x.MemberType == System.Reflection.MemberTypes.Method && x.Name == methName)
                    .Select(x => (System.Reflection.MethodInfo)x)
                    .Where(x => x.GetGenericArguments().Length == argNum)
                    .First();

            if (argNum > 0)
                return method.MakeGenericMethod(typeArgs);
            else
                return method;

        }

        private ContextCallableDelegate<TInstance> CreateAction(System.Reflection.MethodInfo target)
        {
            var method = (Action<TInstance>)Delegate.CreateDelegate(typeof(Action<TInstance>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) => 
                {
                    method(inst);
                    return null;
                });
                
        }

        private ContextCallableDelegate<TInstance> CreateAction<T>(System.Reflection.MethodInfo target)
        {
            var method = (Action<TInstance, T>)Delegate.CreateDelegate(typeof(Action<TInstance, T>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) => 
                {
                    method(inst, ConvertParam<T>(args[0]));
                    return null;
                });
        }

        private ContextCallableDelegate<TInstance> CreateAction<T1, T2>(System.Reflection.MethodInfo target)
        {
            var method = (Action<TInstance, T1, T2>)Delegate.CreateDelegate(typeof(Action<TInstance, T1, T2>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                method(inst, ConvertParam<T1>(args[0]), ConvertParam<T2>(args[1]));
                return null;
            });
        }

        private ContextCallableDelegate<TInstance> CreateAction<T1, T2, T3>(System.Reflection.MethodInfo target)
        {
            var method = (Action<TInstance, T1, T2, T3>)Delegate.CreateDelegate(typeof(Action<TInstance, T1, T2, T3>), target);
            
            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                method(inst, ConvertParam<T1>(args[0]), ConvertParam<T2>(args[1]), ConvertParam<T3>(args[2]));
                return null;
            });
        }

        private ContextCallableDelegate<TInstance> CreateAction<T1, T2, T3, T4>(System.Reflection.MethodInfo target)
        {
            var method = (Action<TInstance, T1,T2,T3,T4>)Delegate.CreateDelegate(typeof(Action<TInstance, T1,T2,T3,T4>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                method(inst, ConvertParam<T1>(args[0]), ConvertParam<T2>(args[1]), ConvertParam<T3>(args[2]), ConvertParam<T4>(args[3]));
                return null;
            });
        }

        private ContextCallableDelegate<TInstance> CreateFunction<TRet>(System.Reflection.MethodInfo target)
        {
            var method = (Func<TInstance, TRet>)Delegate.CreateDelegate(typeof(Func<TInstance, TRet>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                return ConvertReturnValue(method(inst));
            });

        }

        private ContextCallableDelegate<TInstance> CreateFunction<T, TRet>(System.Reflection.MethodInfo target)
        {
            var method = (Func<TInstance, T, TRet>)Delegate.CreateDelegate(typeof(Func<TInstance, T, TRet>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                return ConvertReturnValue(method(inst, ConvertParam<T>(args[0])));
            });
        }

        private ContextCallableDelegate<TInstance> CreateFunction<T1, T2, TRet>(System.Reflection.MethodInfo target)
        {
            var method = (Func<TInstance, T1, T2, TRet>)Delegate.CreateDelegate(typeof(Func<TInstance, T1, T2, TRet>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                return ConvertReturnValue(method(inst, ConvertParam<T1>(args[0]), ConvertParam<T2>(args[1])));
            });
        }

        private ContextCallableDelegate<TInstance> CreateFunction<T1, T2, T3, TRet>(System.Reflection.MethodInfo target)
        {
            var method = (Func<TInstance, T1, T2, T3, TRet>)Delegate.CreateDelegate(typeof(Func<TInstance, T1, T2, T3, TRet>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                return ConvertReturnValue(method(inst, 
                    ConvertParam<T1>(args[0]),
                    ConvertParam<T2>(args[1]), 
                    ConvertParam<T3>(args[2])));
            });
        }

        private ContextCallableDelegate<TInstance> CreateFunction<T1, T2, T3, T4, TRet>(System.Reflection.MethodInfo target)
        {
            var method = (Func<TInstance, T1, T2, T3, T4, TRet>)Delegate.CreateDelegate(typeof(Func<TInstance, T1, T2, T3, T4, TRet>), target);

            return new ContextCallableDelegate<TInstance>((inst, args) =>
            {
                return ConvertReturnValue(method(inst, 
                    ConvertParam<T1>(args[0]),
                    ConvertParam<T2>(args[1]),
                    ConvertParam<T3>(args[2]), 
                    ConvertParam<T4>(args[3])));
            });

        }

        private T ConvertParam<T>(IValue value)
        {
            return ValueMarshaler.IValueToCLRType<T>(value);
        }

        private IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ValueMarshaler.CLRTypeToIValue(param);
        }

        private struct MethodDefinition
        {
            public string Name;
            public string Alias;
            public bool IsFunction;
            public ParameterDefinition[] Params;

            public int ArgCount
            {
                get
                {
                    return Params != null ? Params.Length : 0;
                }
            }
        }

        private struct ParameterDefinition
        {
            public bool IsByValue;
            public bool HasDefaultValue;
            public object DefaultValue;
        }

        private struct InternalMethInfo
        {
            public ContextCallableDelegate<TInstance> method;
            public MethodDefinition methodDef;
        }

    }
}
