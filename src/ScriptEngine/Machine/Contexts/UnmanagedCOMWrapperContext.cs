/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Rcw;
using OneScript.Values;
using ScriptEngine.Machine.Rcw;

namespace ScriptEngine.Machine.Contexts
{
    public class UnmanagedCOMWrapperContext : COMWrapperContext, IDebugPresentationAcceptor
    {
        private const uint E_DISP_MEMBERNOTFOUND = 0x80020003;

        private readonly RcwMembersMetadataCollection<RcwPropertyMetadata> _props;
        private readonly RcwMembersMetadataCollection<RcwMethodMetadata> _methods;
        private readonly bool _isCollection;

        public UnmanagedCOMWrapperContext(object instance) : base(instance)
        {
            InitByInstance();
            var md = new RcwMetadata(instance);
            _props = md.Properties;
            _methods = md.Methods;
            _isCollection = md.IsCollection;
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

        public override int Count()
        {
            if (!_isCollection)
                return 0;
            else
                return ((IEnumerable<IValue>) this).Count();
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            if (!_isCollection)
                throw RuntimeException.IteratorIsNotDefined();

            var comEnumerator = ((IEnumerable)Instance).GetEnumerator();

            while (comEnumerator.MoveNext())
            {
                yield return CreateIValue(comEnumerator.Current);
            }
        }

        public override bool IsIndexed => _props.Count > 0;
            
        public override int GetPropCount() => _props.Count;

        public override string GetPropName(int propNum) 
            => _props[propNum].Name;

        public override int GetPropertyNumber(string name)
        {
            if(!TryFindProperty(name, out var md))
                throw PropertyAccessException.PropNotFoundException(name);

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
            catch (MissingMemberException)
            {
                throw PropertyAccessException.PropNotFoundException(prop.Name);
            }
            catch (MemberAccessException)
            {
                throw PropertyAccessException.PropIsNotReadableException(prop.Name);
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
                    if(newVal is BslDateValue dateVal)
                    {
                        var date = (DateTime)dateVal;
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
                throw OneScript.Commons.PropertyAccessException.PropNotFoundException(prop.Name);
            }
            catch (System.MemberAccessException)
            {
                throw OneScript.Commons.PropertyAccessException.PropIsNotWritableException(prop.Name);
            }
        }

        public override int GetMethodNumber(string name)
        {
            if (!TryFindMethod(name, out var md))
                throw RuntimeException.MethodNotFoundException(name);
            
            return _methods.IndexOf(md);
        }

        public override BslMethodInfo GetMethodInfo(int methodNumber)
        {
            //TODO: Доработать RcwMethodMetadata
            return BslMethodBuilder.Create().Build();
        }

        private MethodSignature GetMethodDescription(int methodNumber)
        {
            //TODO: Доработать RcwMethodMetadata
            return new MethodSignature();
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

            if (!(method.IsFunction ?? true))
                throw RuntimeException.UseProcAsAFunction();

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

        private bool TryFindMethod(string name, out RcwMethodMetadata md)
        {
            if (_methods.Names.TryGetValue(name, out md))
                return true;

            if (!DispatchUtility.TryGetDispId(Instance, name, out var dispatchId))
                return false;
            
            _methods.Add(new RcwMethodMetadata(name, dispatchId, null));
            md = _methods.DispatchIds[dispatchId];
            
            return true;
        }

        private bool TryFindProperty(string name, out RcwPropertyMetadata md)
        {
            if (_props.Names.TryGetValue(name, out md))
                return true;

            if (!DispatchUtility.TryGetDispId(Instance, name, out var dispatchId))
                return false;
            
            _props.Add(new RcwPropertyMetadata(name, dispatchId));
            md = _props.DispatchIds[dispatchId];
            
            return true;
        }

        public void Accept(IDebugValueVisitor visitor)
        {
            visitor.ShowProperties(this);
            if(_isCollection)
                visitor.ShowCollectionItems(this);
        }
    }
}
//#endif