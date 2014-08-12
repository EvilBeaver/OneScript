using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.ComponentModel
{
    abstract public class ContextBase : ComponentBase, IRuntimeContextInstance
    {
        public override IRuntimeContextInstance AsObject()
        {
            return this;
        }

        protected virtual IValue GetPropertyValueInternal(int index)
        {
            throw new ArgumentException();
        }

        protected virtual void SetPropertyValueInternal(int index, IValue newValue)
        {
            throw new ArgumentException();
        }

        #region IRuntimeContextInstance members
        public virtual bool IsIndexed
        {
            get { return true; }
        }

        public virtual int GetPropCount()
        {
            return 0;
        }

        public virtual int FindProperty(string name)
        {
            throw ContextAccessException.PropNotFound(name);
        }

        public virtual string GetPropertyName(int index, NameRetrievalMode mode = NameRetrievalMode.Default)
        {
            throw new ArgumentException();
        }

        public virtual IValue GetIndexedValue(IValue index)
        {
            var propIdx = FindProperty(index.AsString());
            return GetPropertyValue(propIdx);
        }

        public virtual void SetIndexedValue(IValue index, IValue newValue)
        {
            var propIdx = FindProperty(index.AsString());
            SetPropertyValue(propIdx, newValue);
        }

        public IValue GetPropertyValue(int index)
        {
            if (IsPropReadable(index))
                return GetPropertyValueInternal(index);
            else
                throw ContextAccessException.PropIsNotReadable(GetPropertyName(index));
        }

        public void SetPropertyValue(int index, IValue newValue)
        {
            if (IsPropWriteable(index))
                SetPropertyValueInternal(index, newValue);
            else
                throw ContextAccessException.PropIsNotWritable(GetPropertyName(index));
        }
        
        public virtual bool IsPropReadable(int index)
        {
            throw new ArgumentException();
        }
       
        public virtual bool IsPropWriteable(int index)
        {
            throw new ArgumentException();
        }

        public virtual bool DynamicMethodSignatures
        {
            get { return false; }
        }

        public virtual int GetMethodsCount()
        {
            return 0;
        }

        public virtual int FindMethod(string name)
        {
            throw ContextAccessException.MethodNotFound(name);
        }

        public virtual string GetMethodName(int index, NameRetrievalMode mode = NameRetrievalMode.Default)
        {
            throw new ArgumentException();
        }

        public virtual bool HasReturnValue(int index)
        {
            throw new ArgumentException();
        }

        public virtual int GetParametersCount(int index)
        {
            throw new ArgumentException();
        }

        public virtual bool GetDefaultValue(int methodIndex, int paramIndex, out IValue defaultValue)
        {
            throw new ArgumentException();
        }

        public virtual void CallAsProcedure(int index, IValue[] args)
        {
            throw new ArgumentException();
        }

        public virtual IValue CallAsFunction(int index, IValue[] args)
        {
            throw new ArgumentException();
        } 

        #endregion
    }
}
