using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [AttributeUsage(AttributeTargets.Method)]
    class ContextMethodAttribute : Attribute
    {
        string _name;

        public ContextMethodAttribute(string name)
        {
            _name = name;
        }

        public string GetName()
        {
            return _name;
        }

        public bool IsFunction { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    class ByRefAttribute : Attribute
    {
    }

    delegate IValue ContextCallableDelegate<TInstance>(TInstance instance, IValue[] args);

    class ContextMethodsMapper<TInstance>
    {
        private List<InternalMethInfo> _methodPtrs = new List<InternalMethInfo>();
        
        public ContextMethodsMapper()
        {
            MapType(typeof(TInstance));
        }

        public ContextCallableDelegate<TInstance> GetMethod(int number)
        {
            return _methodPtrs[number].method;
        }

        public ScriptEngine.Machine.MethodInfo GetMethodInfo(int number)
        {
            return _methodPtrs[number].methodInfo;
        }

        public int FindMethod(string name)
        {
            name = name.ToLower();
            var idx = _methodPtrs.FindIndex(x => x.methodInfo.Name == name);
            if (idx < 0)
            {
                throw RuntimeException.MethodNotFoundException(name);
            }

            return idx;
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
                            if (paramTypes[i] != typeof(IVariable))
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
                    scriptMethInfo.Name = item.Binding.GetName().ToLower();
                    scriptMethInfo.Params = paramDefs;

                    _methodPtrs.Add(new InternalMethInfo()
                    {
                        method = methPtr,
                        methodInfo = scriptMethInfo
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
            return ContextValuesMarshaller.ConvertParam<T>(value);
        }

        private IValue ConvertReturnValue<TRet>(TRet param)
        {
            return ContextValuesMarshaller.ConvertReturnValue<TRet>(param);
        }


        private struct InternalMethInfo
        {
            public ContextCallableDelegate<TInstance> method;
            public ScriptEngine.Machine.MethodInfo methodInfo;
        }

    }
}
