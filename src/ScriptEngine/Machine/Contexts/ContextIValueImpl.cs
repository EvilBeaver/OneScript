﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Dynamic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Types;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class ContextIValueImpl : BslObjectValue, IRuntimeContextInstance, ISystemTypeAcceptor
    {
        private TypeDescriptor _type;

        protected ContextIValueImpl() : this(BasicTypes.UnknownType)
        {
        }
        
        protected ContextIValueImpl(TypeDescriptor type)
        {
            _type = type;
        }

        void ISystemTypeAcceptor.AssignType(TypeDescriptor type)
        {
            _type = type;
        }
        
        protected void DefineType(TypeDescriptor type)
        {
            _type = type;
        }

        public override string ToString()
        {
            if (_type == BasicTypes.UnknownType)
                TryDetermineOwnType();
            
            return _type.Name;
        }
        
        #region IValue Members

        public override TypeDescriptor SystemType
        {
            get
            {
                if (_type != BasicTypes.UnknownType) 
                    return _type;
                
                if (!TryDetermineOwnType())
                {
                    throw new InvalidOperationException($"Type {GetType()} is not defined");
                }

                return _type;
            }
        }

        private bool TryDetermineOwnType()
        {
            var mgr = MachineInstance.Current?.TypeManager;
            if (mgr?.IsKnownType(GetType()) ?? false)
            {
                _type = mgr.GetTypeByFrameworkType(GetType());
                return true;
            }

            return false;
        }
        
        #endregion

        #region IComparable<IValue> Members

        public override int CompareTo(IValue other)
        {
            throw RuntimeException.ComparisonNotSupportedException();
        }

        #endregion

        #region IEquatable<IValue> Members

        public override bool Equals(IValue other)
        {
            if (!(other is BslObjectValue _))
                return false;

            return ReferenceEquals(this, other);
        }

        #endregion

        #region IRuntimeContextInstance Members

        public virtual bool IsIndexed => false;

        public virtual bool DynamicMethodSignatures => false;

        public virtual IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public virtual void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public virtual int GetPropertyNumber(string name)
        {
            throw PropertyAccessException.PropNotFoundException(name);
        }
        public virtual bool IsPropReadable(int propNum)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsPropWritable(int propNum)
        {
            throw new NotImplementedException();
        }
        public virtual IValue GetPropValue(int propNum)
        {
            throw new NotImplementedException();
        }
        public virtual void SetPropValue(int propNum, IValue newVal)
        {
            throw new NotImplementedException();
        }

        public virtual int GetPropCount()
        {
            throw new NotImplementedException();
        }

        public virtual string GetPropName(int propNum)
        {
            throw new NotImplementedException();
        }

        public virtual int GetMethodsCount()
        {
            throw new NotImplementedException();
        }

        public virtual int GetMethodNumber(string name)
        {
            throw RuntimeException.MethodNotFoundException(name);
        }
        
        public virtual BslMethodInfo GetMethodInfo(int methodNumber)
        {
            throw new NotImplementedException();
        }

        public virtual BslPropertyInfo GetPropertyInfo(int propertyNumber)
        {
            throw new NotImplementedException();
        }
        
        public virtual void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            throw new NotImplementedException();
        }
        public virtual void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region DynamicObject members 
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var propIdx = GetPropertyNumber(binder.Name);
                if (!IsPropReadable(propIdx))
                {
                    result = null;
                    return false;
                }

                result = ContextValuesMarshaller.ConvertToClrObject(GetPropValue(propIdx));
                return true;
            }
            catch (PropertyAccessException)
            {
                result = null;
                return false;
            }
            catch (ValueMarshallingException)
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                var propIdx = GetPropertyNumber(binder.Name);
                if (!IsPropWritable(propIdx))
                {
                    return false;
                }

                SetPropValue(propIdx, ContextValuesMarshaller.ConvertDynamicValue(value));
                return true;
            }
            catch (PropertyAccessException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (!IsIndexed)
            {
                result = null;
                return false;
            }

            var index = ContextValuesMarshaller.ConvertDynamicIndex(indexes[0]);
            result = ContextValuesMarshaller.ConvertToClrObject(GetIndexedValue(index));
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (!IsIndexed)
            {
                return false;
            }

            var index = ContextValuesMarshaller.ConvertDynamicIndex(indexes[0]);
            SetIndexedValue(index, ContextValuesMarshaller.ConvertDynamicValue(value));
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            int methIdx;
            try
            {
                methIdx = GetMethodNumber(binder.Name);
            }
            catch (MethodAccessException)
            {
                result = null;
                return false;
            }

            var parameters = GetMethodInfo(methIdx).GetParameters();
            if (args.Length > parameters.Length)
                throw RuntimeException.TooManyArgumentsPassed();

            var valueArgs = new IValue[parameters.Length];
            var passedArgs = args.Select(x => ContextValuesMarshaller.ConvertDynamicValue(x)).ToArray();
            
            for (int i = 0; i < valueArgs.Length; i++)
            {
                if (i < passedArgs.Length)
                    valueArgs[i] = passedArgs[i];
                else
                    valueArgs[i] = ValueFactory.CreateInvalidValueMarker();
            }

            CallAsFunction(methIdx, valueArgs, out IValue methResult);
            result = methResult == null ? null : ContextValuesMarshaller.ConvertToClrObject(methResult);

            return true;
        }

        #endregion
        
        public override int CompareTo(BslValue other)
        {
            if (other.GetType() == GetType())
            {
                if (this.Equals(other))
                {
                    return 0;
                }
                else
                {
                    throw RuntimeException.ComparisonNotSupportedException();
                }
            }
            else
            {
                return this.GetType().ToString().CompareTo(other.GetType().ToString());
            }
        }

        public override bool Equals(BslValue other)
        {
            if (other == null)
                return false;

            return ReferenceEquals(this, other);
        }
    }

}
