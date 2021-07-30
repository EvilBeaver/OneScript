/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Dynamic;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
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
            return _type.Name;
        }
        
        #region IValue Members

        public override TypeDescriptor SystemType
        {
            get
            {
                if (_type == BasicTypes.UnknownType)
                {
                    var mgr = MachineInstance.Current.TypeManager;
                    if (mgr.IsKnownType(this.GetType()))
                    {
                        _type = mgr.GetTypeByFrameworkType(this.GetType());
                    }
                    else
                    {
                        throw new InvalidOperationException($"Type {GetType()} is not defined");
                    }
                }

                return _type;
            }
        }
        
        #endregion

        #region IComparable<IValue> Members

        public override int CompareTo(IValue other)
        {
            if (other.SystemType.Equals(this.SystemType))
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
                return this.SystemType.ToString().CompareTo(other.SystemType.ToString());
            }
        }

        #endregion

        #region IEquatable<IValue> Members

        public override bool Equals(IValue other)
        {
            if (other == null)
                return false;

            return other.SystemType.Equals(this.SystemType) && Object.ReferenceEquals(this.AsObject(), other.AsObject());
        }

        #endregion

        #region IRuntimeContextInstance Members

        public virtual bool IsIndexed
        {
            get { return false; }
        }

        public virtual bool DynamicMethodSignatures
        {
            get { return false; }
        }

        public virtual IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public virtual void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public virtual int FindProperty(string name)
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

        public virtual int FindMethod(string name)
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
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                var propIdx = FindProperty(binder.Name);
                if (!IsPropReadable(propIdx))
                {
                    result = null;
                    return false;
                }

                result = ContextValuesMarshaller.ConvertToCLRObject(GetPropValue(propIdx));
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
                var propIdx = FindProperty(binder.Name);
                if (!IsPropWritable(propIdx))
                {
                    return false;
                }

                SetPropValue(propIdx, ContextValuesMarshaller.ConvertReturnValue(value, value.GetType()));

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

            var index = ContextValuesMarshaller.ConvertReturnValue(indexes[0], indexes[0].GetType());
            result = ContextValuesMarshaller.ConvertToCLRObject(GetIndexedValue(index));
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (!IsIndexed)
            {
                return false;
            }

            var index = ContextValuesMarshaller.ConvertReturnValue(indexes[0], indexes[0].GetType());
            SetIndexedValue(index, ContextValuesMarshaller.ConvertReturnValue(value, value.GetType()));
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            int methIdx;
            try
            {
                methIdx = FindMethod(binder.Name);
            }
            catch (MethodAccessException)
            {
                result = null;
                return false;
            }

            var parameters = GetMethodInfo(methIdx).GetParameters();
            var valueArgs = new IValue[parameters.Length];
            var passedArgs = args.Select(x => ContextValuesMarshaller.ConvertReturnValue(x, x.GetType())).ToArray();
            for (int i = 0; i < valueArgs.Length; i++)
            {
                if (i < passedArgs.Length)
                    valueArgs[i] = passedArgs[i];
                else
                    valueArgs[i] = ValueFactory.CreateInvalidValueMarker();
            }

            IValue methResult;
            CallAsFunction(methIdx, valueArgs, out methResult);
            result = methResult == null? null : ContextValuesMarshaller.ConvertToCLRObject(methResult);

            return true;

        }

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
