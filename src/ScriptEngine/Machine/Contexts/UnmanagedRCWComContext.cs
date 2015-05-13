#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    class UnmanagedRCWComContext : COMWrapperContext
    {
        private Type _dispatchedType;
        private object _instance;

        private const uint E_DISP_MEMBERNOTFOUND = 0x80020003;
        private bool? _isIndexed;
        private Dictionary<string, int> _dispIdCache = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<int, MemberInfo> _membersCache = new Dictionary<int, MemberInfo>();
        private Dictionary<int, MethodInfo> _methodBinding = new Dictionary<int, MethodInfo>();

        public UnmanagedRCWComContext(object instance)
        {
            _instance = instance;
            InitByInstance();
        }

        private void InitByInstance()
        {
            if (!DispatchUtility.ImplementsIDispatch(_instance))
            {
                _instance = null;
                throw new RuntimeException("Объект не реализует IDispatch.");
            }

            try
            {
                _dispatchedType = DispatchUtility.GetType(_instance, false);
            }
            catch
            {
                _instance = null;
                throw;
            }
        }

        protected override void Dispose(bool manualDispose)
        {
            base.Dispose(manualDispose);
            
            _membersCache.Clear();
            _methodBinding.Clear();
            _dispIdCache.Clear();

            if (_instance != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_instance);
                _instance = null;
            }
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            var comType = _instance.GetType();
            System.Collections.IEnumerator comEnumerator;

            try
            {

                comEnumerator = (System.Collections.IEnumerator)comType.InvokeMember("[DispId=-4]",
                                        BindingFlags.InvokeMethod,
                                        null,
                                        _instance,
                                        new object[0]);
            }
            catch (TargetInvocationException e)
            {
                uint hr = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e.InnerException);
                if (hr == E_DISP_MEMBERNOTFOUND)
                {
                    throw RuntimeException.IteratorIsNotDefined();
                }
                else
                {
                    throw;
                }
            }

            while (comEnumerator.MoveNext())
            {
                yield return CreateIValue(comEnumerator.Current);
            }

        }

        public override bool IsIndexed
        {
            get
            {
                if (_isIndexed == null)
                {
                    try
                    {

                        var comValue = _instance.GetType().InvokeMember("[DispId=0]",
                                                BindingFlags.InvokeMethod | BindingFlags.GetProperty,
                                                null,
                                                _instance,
                                                new object[0]);
                        _isIndexed = true;
                    }
                    catch (TargetInvocationException e)
                    {
                        uint hr = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e.InnerException);
                        if (hr == E_DISP_MEMBERNOTFOUND)
                        {
                            _isIndexed = false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                return (bool)_isIndexed;
            }
        }

        public override object UnderlyingObject
        {
            get
            {
                return _instance;
            }
        }

        public override int FindProperty(string name)
        {
            int dispId;
            if (!_dispIdCache.TryGetValue(name, out dispId))
            {
                if (DispatchUtility.TryGetDispId(_instance, name, out dispId))
                {
                    if (_dispatchedType != null)
                    {
                        var memberInfo = _dispatchedType.GetMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
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
            if (_dispatchedType != null)
            {
                var propInfo = (PropertyInfo)_membersCache[propNum];
                return propInfo.CanRead;
            }
            else
            {
                return true;
            }
        }

        public override bool IsPropWritable(int propNum)
        {
            if (_dispatchedType != null)
            {
                var propInfo = (PropertyInfo)_membersCache[propNum];
                return propInfo.CanWrite;
            }
            else
            {
                return true;
            }
        }

        public override IValue GetPropValue(int propNum)
        {
            try
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
            catch (System.MissingMemberException)
            {
                throw RuntimeException.PropNotFoundException("dispid[" + propNum.ToString() + "]");
            }
            catch (System.MemberAccessException)
            {
                throw RuntimeException.PropIsNotReadableException("dispid[" + propNum.ToString() + "]");
            }
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            try
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
            catch (System.MissingMemberException)
            {
                throw RuntimeException.PropNotFoundException("dispid[" + propNum.ToString() + "]");
            }
            catch (System.MemberAccessException)
            {
                throw RuntimeException.PropIsNotWritableException("dispid[" + propNum.ToString() + "]");
            }
        }

        public override int FindMethod(string name)
        {
            int dispId;
            if (!_dispIdCache.TryGetValue(name, out dispId))
            {
                if (DispatchUtility.TryGetDispId(_instance, name, out dispId))
                {
                    if (_dispatchedType != null)
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
            if (_dispatchedType != null)
                return GetReflectableMethod(methodNumber);
            else
                return new MethodInfo();
        }

        private MethodInfo GetReflectableMethod(int methodNumber)
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
                try
                {
                    DispatchUtility.Invoke(_instance, methodNumber, MarshalArguments(arguments));
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.MethodNotFoundException("dispid[" + methodNumber.ToString() + "]");
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            try
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
            catch (System.MissingMemberException)
            {
                throw RuntimeException.MethodNotFoundException("dispid[" + methodNumber.ToString() + "]");
            }
        }

    }
}
#endif