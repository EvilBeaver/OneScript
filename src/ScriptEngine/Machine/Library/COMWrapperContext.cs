using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("COMОбъект")]
    class COMWrapperContext : PropertyNameIndexAccessor, ICollectionContext, IDisposable
    {
        private Type _dispatchedType;
        private object _instance;
        private bool? _isEnumerable;
        private Dictionary<string, int> _dispIdCache = new Dictionary<string,int>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<int, MemberInfo> _membersCache = new Dictionary<int, MemberInfo>();
        private Dictionary<int, MethodInfo> _methodBinding = new Dictionary<int, MethodInfo>();
        
        public COMWrapperContext(string progId, IValue[] arguments)
        {
            var type = Type.GetTypeFromProgID(progId, true);
            var args = MarshalArguments(arguments);
            _instance = Activator.CreateInstance(type, args);
            InitByInstance();
        }

        private void InitByInstance()
        {
            if (!DispatchUtility.ImplementsIDispatch(_instance))
            {
                _instance = null;
                throw new RuntimeException("The object doesn't implement IDispatch.");
            }

            try
            {
                _dispatchedType = DispatchUtility.GetType(_instance, true);
            }
            catch
            {
                _instance = null;
                throw;
            }
        }

        public COMWrapperContext(object instance)
        {
            _instance = instance;
            InitByInstance();
        }

        private object[] MarshalArguments(IValue[] arguments)
        {
            var args = arguments.Select(x => MarshalIValue(x)).ToArray();
            return args;
        }

        private object MarshalIValue(IValue val)
        {
            object result;
            switch (val.DataType)
            {
                case Machine.DataType.Boolean:
                    result = val.AsBoolean();
                    break;
                case Machine.DataType.Date:
                    result = val.AsDate();
                    break;
                case Machine.DataType.Number:
                    result = val.AsNumber();
                    break;
                case Machine.DataType.String:
                    result = val.AsString();
                    break;
                case Machine.DataType.Undefined:
                    result = null;
                    break;
                case Machine.DataType.Object:
                    result = val.AsObject();
                    if (result.GetType() == typeof(COMWrapperContext))
                        result = ((COMWrapperContext)result).UnderlyingObject;
                    break;
                default:
                    throw new RuntimeException("Unsupported type for COM marshalling");
            }

            return result;
        }

        private IValue CreateIValue(object objParam)
        {
            var type = objParam.GetType();
            if (type == typeof(string))
            {
                return ValueFactory.Create((string)objParam);
            }
            else if (type == typeof(int))
            {
                return ValueFactory.Create((int)objParam);
            }
            else if (type == typeof(double))
            {
                return ValueFactory.Create((double)objParam);
            }
            else if (type == typeof(DateTime))
            {
                return ValueFactory.Create((DateTime)objParam);
            }
            else if (type == typeof(bool))
            {
                return ValueFactory.Create((bool)objParam);
            }
            else if(DispatchUtility.ImplementsIDispatch(objParam))
            {
                var ctx = new COMWrapperContext(objParam);
                return ValueFactory.Create(ctx);
            }
            else
            {
                throw new RuntimeException("Type " + type + " can't be converted to a supported value");
            }
        }

        private object UnderlyingObject
        {
            get
            {
                return _instance;
            }
        }

        #region ICollectionContext Members

        public bool IsEnumerable()
        {
            if (_isEnumerable == null)
            {
                var enumMethod = _dispatchedType.GetMethod("GetEnumerator");
                if (enumMethod != null && enumMethod.ReturnType == typeof(System.Collections.IEnumerator))
                {
                    _isEnumerable = true;
                }
                else
                {
                    _isEnumerable = false;
                }
            }

            return (bool)_isEnumerable;
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #region IEnumerable<IValue> Members

        public IEnumerator<IValue> GetEnumerator()
        {
            if (IsEnumerable())
            {
                var enumerator = (System.Collections.IEnumerator)_dispatchedType.InvokeMember("GetEnumerator", BindingFlags.InvokeMethod, null, _instance, null);
                while (enumerator.MoveNext())
                {
                    yield return CreateIValue(enumerator.Current);
                }
            }
            else
            {
                throw RuntimeException.IteratorIsNotDefined();
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion        

        #region IDisposable Members

        private void Dispose(bool manualDispose)
        {
            if (manualDispose)
            {
                GC.SuppressFinalize(this);
                _membersCache.Clear();
                _methodBinding.Clear();
                _dispIdCache.Clear();
            }

            IDisposable instanceRef = _instance as IDisposable;
            if (instanceRef != null)
            {
                instanceRef.Dispose();
                _instance = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~COMWrapperContext()
        {
            Dispose(false);
        }

        #endregion

        public override int FindProperty(string name)
        {
            int dispId;
            if (!_dispIdCache.TryGetValue(name, out dispId))
            {
                if (DispatchUtility.TryGetDispId(_instance, name, out dispId))
                {
                    var memberInfo = _dispatchedType.GetMember(name);
                    if (memberInfo.Length == 0 || !(memberInfo[0].MemberType == MemberTypes.Property))
                    {
                        throw RuntimeException.PropNotFoundException(name);
                    }
                    else
                    {
                        _membersCache.Add(dispId, memberInfo[0]);
                        _dispIdCache.Add(name, dispId);
                    }
                }
                else
                {
                    throw RuntimeException.PropNotFoundException(name);
                }
            }

            return dispId;
        }

        public override bool IsPropReadable(int propNum)
        {
            var propInfo = (PropertyInfo)_membersCache[propNum];
            return propInfo.CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            var propInfo = (PropertyInfo)_membersCache[propNum];
            return propInfo.CanWrite;
        }

        public override IValue GetPropValue(int propNum)
        {
            try
            {
                var result = DispatchUtility.Invoke(_instance, propNum, null);
                return CreateIValue(result);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            try
            {
                DispatchUtility.InvokeSetProperty(_instance, propNum, MarshalIValue(newVal));
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            int dispId;
            if (!_dispIdCache.TryGetValue(name, out dispId))
            {
                if (DispatchUtility.TryGetDispId(_instance, name, out dispId))
                {
                    var memberInfo = _dispatchedType.GetMember(name);
                    if (memberInfo.Length == 0 || !(memberInfo[0].MemberType == MemberTypes.Method || memberInfo[0].MemberType == MemberTypes.Property))
                    {
                        throw RuntimeException.MethodNotFoundException(name);
                    }
                    else
                    {
                        _membersCache.Add(dispId, memberInfo[0]);
                        _dispIdCache.Add(name, dispId);
                    }
                }
                else
                {
                    throw RuntimeException.MethodNotFoundException(name);
                }
            }

            return dispId;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return GetMethodDescription(methodNumber);
        }

        private MethodInfo GetMethodDescription(int methodNumber)
        {
            MethodInfo methodInfo;
            if (!_methodBinding.TryGetValue(methodNumber, out methodInfo))
            {
                var memberInfo = _membersCache[methodNumber];

                methodInfo = new MethodInfo();
                methodInfo.Name = memberInfo.Name;

                var reflectedMethod = memberInfo as System.Reflection.MethodInfo;

                if (reflectedMethod != null)
                {
                    methodInfo.IsFunction = reflectedMethod.ReturnType != typeof(void);
                    var reflectionParams = reflectedMethod.GetParameters();
                    FillMethodInfoParameters(ref methodInfo, reflectionParams);
                }
                else
                {
                    var reflectedProperty = memberInfo as System.Reflection.PropertyInfo;
                    if (reflectedProperty != null)
                    {
                        var reflectionParams = reflectedProperty.GetIndexParameters();
                        if (reflectionParams.Length == 0)
                        {
                            throw RuntimeException.IndexedAccessIsNotSupportedException();
                        }

                        methodInfo.IsFunction = reflectedProperty.CanRead;
                        FillMethodInfoParameters(ref methodInfo, reflectionParams);
                    }
                    else
                    {
                        throw RuntimeException.IndexedAccessIsNotSupportedException();
                    }
                }

                _methodBinding.Add(methodNumber, methodInfo);
            }
            return methodInfo;
        }

        private static void FillMethodInfoParameters(ref MethodInfo methodInfo, System.Reflection.ParameterInfo[] reflectionParams)
        {
            methodInfo.Params = new ParameterDefinition[reflectionParams.Length];
            for (int i = 0; i < reflectionParams.Length; i++)
            {
                var reflectedParam = reflectionParams[i];
                var param = new ParameterDefinition();
                param.HasDefaultValue = reflectedParam.IsOptional;
                param.IsByValue = !reflectedParam.IsOut;
                methodInfo.Params[i] = param;
            }
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            try
            {
                DispatchUtility.Invoke(_instance, methodNumber, MarshalArguments(arguments));
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            try
            {
                var result = DispatchUtility.Invoke(_instance, methodNumber, MarshalArguments(arguments));
                retValue = CreateIValue(result);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue[] args)
        {
            return new COMWrapperContext(args[0].AsString(), args.Skip(1).ToArray());
        }

    }
}
