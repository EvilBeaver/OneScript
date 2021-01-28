/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Dynamic;
using System.Linq;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class ContextIValueImpl : DynamicObject, IRuntimeContextInstance, IValue
    {
        private TypeDescriptor _type;

        protected ContextIValueImpl()
        {
        }

        protected ContextIValueImpl(TypeDescriptor type)
        {
            DefineType(type);
        }

        protected void DefineType(TypeDescriptor type)
        {
            _type = type;
        }

        public override string ToString()
        {
            return _type?.Name ?? base.ToString();
        }
        
        #region IValue Members

        public DataType DataType => DataType.Object;

        public TypeDescriptor SystemType
        {
            get
            {
                if (_type == default)
                {
                    if (TypeManager.IsKnownType(this.GetType()))
                    {
                        _type = TypeManager.GetTypeByFrameworkType(this.GetType());
                    }
                    else
                    {
                        throw new InvalidOperationException($"Type {GetType()} is not defined");
                    }
                }

                return _type;
            }
        }

        public decimal AsNumber()
        {
            throw RuntimeException.ConvertToNumberException();
        }

        public DateTime AsDate()
        {
            throw RuntimeException.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            throw RuntimeException.ConvertToBooleanException();
        }

        public virtual string AsString()
        {
            return SystemType.Name;
        }

        public IRuntimeContextInstance AsObject()
        {
            return this;
        }

        public IValue GetRawValue()
        {
            return this;
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
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

        public virtual bool Equals(IValue other)
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
            throw RuntimeException.PropNotFoundException(name);
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
        public virtual MethodInfo GetMethodInfo(int methodNumber)
        {
            throw new NotImplementedException();
        }
        public virtual VariableInfo GetPropertyInfo(int propertyNumber)
        {
            return new VariableInfo
            {
                Identifier = GetPropName(propertyNumber),
                CanGet = IsPropReadable(propertyNumber),
                CanSet = IsPropWritable(propertyNumber),
                Index = propertyNumber,
                Type = SymbolType.ContextProperty
            };
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
                if (IsPropWritable(propIdx))
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

            var methInfo = GetMethodInfo(methIdx);
            var valueArgs = new IValue[methInfo.Params.Length];
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
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ScriptConstructorAttribute : Attribute
    {
        public string Name { get; set; }
        
        [Obsolete("Use TypeActivationContext as first parameter in Constructor")]
        public bool ParametrizeWithClassName { get; set; }
    }
}
