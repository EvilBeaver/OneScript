/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Reflection;
using ScriptEngine.Machine.Rcw;

namespace ScriptEngine.Machine.Contexts
{
    public class UnmanagedCOMWrapperContext : COMWrapperContext, IDebugPresentationAcceptor
    {
        private const uint E_DISP_MEMBERNOTFOUND = 0x80020003;

        private readonly RcwMembersMetadataCollection<RcwPropertyMetadata> _props;
        private readonly RcwMembersMetadataCollection<RcwMethodMetadata> _methods;

        public UnmanagedCOMWrapperContext(object instance) : base(instance)
        {
            InitByInstance();
            var md = new RcwMetadata(instance);
            _props = md.Properties;
            _methods = md.Methods;
        }

        private void InitByInstance()
        {
            if (!DispatchUtility.ImplementsIDispatch(Instance))
            {
                Instance = null;
                throw new RuntimeException("Объект не реализует IDispatch.");
            }
        }

        protected override void Dispose(bool manualDispose)
        {
            base.Dispose(manualDispose);

            if (Instance != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(Instance);
                Instance = null;
            }
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            var comType = Instance.GetType();
            System.Collections.IEnumerator comEnumerator;

            try
            {

                comEnumerator = (System.Collections.IEnumerator)comType.InvokeMember("[DispId=-4]",
                                        BindingFlags.InvokeMethod,
                                        null,
                                        Instance,
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

        public override bool IsIndexed => _props.Count > 0;
            
        public override int GetPropCount() => _props.Count;

        public override string GetPropName(int propNum) 
            => _props[propNum].Name;

        public override int FindProperty(string name)
        {
            if(!_props.Names.TryGetValue(name, out var md))
                throw RuntimeException.PropNotFoundException(name);

            return _props.IndexOf(md);
        }

        public override bool IsPropReadable(int propNum) => _props[propNum].IsReadable;

        public override bool IsPropWritable(int propNum) => _props[propNum].IsWritable;

        public override IValue GetPropValue(int propNum)
        {
            var prop = _props[propNum];

            var dispId = prop.DispatchId;

            try
            {
                try
                {
                    var result = DispatchUtility.Invoke(Instance, dispId, null);
                    return CreateIValue(result);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.PropNotFoundException(prop.Name);
            }
            catch (System.MemberAccessException)
            {
                throw RuntimeException.PropIsNotReadableException(prop.Name);
            }
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            var prop = _props[propNum];

            var dispId = prop.DispatchId;

            try
            {
                try
                {
                    object argToPass;
                    if(newVal.DataType == Machine.DataType.Date)
                    {
                        var date = newVal.AsDate();
                        if(date == DateTime.MinValue)
                        {
                            argToPass = new DateTime(100, 1, 1); // Min OLEAuth Date
                        }
                        else
                        {
                            argToPass = MarshalIValue(newVal);
                        }
                    }
                    else
                    {
                        argToPass = MarshalIValue(newVal);
                    }
                    DispatchUtility.InvokeSetProperty(Instance, dispId, argToPass);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.PropNotFoundException(prop.Name);
            }
            catch (System.MemberAccessException)
            {
                throw RuntimeException.PropIsNotWritableException(prop.Name);
            }
        }

        public override int FindMethod(string name)
        {
            if (!_methods.Names.TryGetValue(name, out var md))
                throw RuntimeException.MethodNotFoundException(name);
            
            return _methods.IndexOf(md);
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return GetMethodDescription(methodNumber);
        }

        private MethodInfo GetMethodDescription(int methodNumber)
        {
            //TODO: Доработать RcwMethodMetadata
            return new MethodInfo();
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            var method = _methods[methodNumber];

            var dispId = method.DispatchId;

            try
            {
                try
                {
                    DispatchUtility.Invoke(Instance, dispId, MarshalArguments(arguments));
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.MethodNotFoundException(method.Name);
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            var method = _methods[methodNumber];

            var dispId = method.DispatchId;

            try
            {
                try
                {
                    var result = DispatchUtility.Invoke(Instance, dispId, MarshalArguments(arguments));
                    retValue = CreateIValue(result);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException ?? e;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.MethodNotFoundException(method.Name);
            }
        }

        public void Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
        }
    }
}
//#endif