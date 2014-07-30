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

        #region IRuntimeContextInstance members
        public virtual bool IsIndexed
        {
            get { return true; }
        }

        public virtual int GetPropCount()
        {
            return 0;
        }

        public int FindProperty(string name)
        {
            throw ContextAccessException.PropNotFound(name);
        }

        public string GetPropertyName(int index)
        {
            throw new ArgumentException();
        }

        public IValue GetIndexedValue(IValue index)
        {
            var propIdx = FindProperty(index.AsString());
            return GetPropertyValue(propIdx);
        }

        public void SetIndexedValue(IValue index, IValue newValue)
        {
            var propIdx = FindProperty(index.AsString());
            SetPropertyValue(propIdx, newValue);
        }

        public IValue GetPropertyValue(int index)
        {
            throw new ArgumentException();
        }

        public void SetPropertyValue(int index, IValue newValue)
        {
            throw new ArgumentException();
        }

        public bool DynamicMethodSignatures
        {
            get { return false; }
        }

        public int GetMethodsCount()
        {
            return 0;
        }

        public int FindMethod(string name)
        {
            throw ContextAccessException.MethodNotFound(name);
        }

        public string GetMethodName(int index)
        {
            throw new ArgumentException();
        }

        public bool HasReturnValue(int index)
        {
            throw new ArgumentException();
        }

        public int GetParametersCount(int index)
        {
            throw new ArgumentException();
        }

        public bool GetDefaultValue(int methodIndex, int paramIndex, out IValue defaultValue)
        {
            throw new ArgumentException();
        }

        public void CallAsProcedure(int index, IValue[] args)
        {
            throw new ArgumentException();
        }

        public IValue CallAsFunction(int index, IValue[] args)
        {
            throw new ArgumentException();
        } 

        #endregion
    }
}
