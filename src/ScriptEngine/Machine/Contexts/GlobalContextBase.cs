/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    abstract public class GlobalContextBase<T> : IRuntimeContextInstance, IAttachableContext where T : GlobalContextBase<T>
    {
        private readonly ContextMethodsMapper<T> _methods = new ContextMethodsMapper<T>();
        private readonly ContextPropertyMapper<T> _properties = new ContextPropertyMapper<T>();

        protected ContextMethodsMapper<T> Methods
        {
            get { return _methods; }
        }

        protected ContextPropertyMapper<T> Properties
        {
            get { return _properties; }
        }

        #region IRuntimeContextInstance members

        public bool IsIndexed
        {
            get { return false; }
        }

        public bool DynamicMethodSignatures
        {
            get { return false; }
        }

        public IValue GetIndexedValue(IValue index)
        {
            throw new NotImplementedException();
        }

        public void SetIndexedValue(IValue index, IValue val)
        {
            throw new NotImplementedException();
        }

        public virtual int FindProperty(string name)
        {
            return _properties.FindProperty(name);
        }

        public virtual bool IsPropReadable(int propNum)
        {
            return _properties.GetProperty(propNum).CanRead;
        }

        public virtual bool IsPropWritable(int propNum)
        {
            return _properties.GetProperty(propNum).CanWrite;
        }

        public virtual IValue GetPropValue(int propNum)
        {
            try
            {
                return _properties.GetProperty(propNum).Getter((T)this);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public virtual void SetPropValue(int propNum, IValue newVal)
        {
            try
            {
                _properties.GetProperty(propNum).Setter((T)this, newVal);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public int GetPropCount()
        {
            return Properties.Count;
        }

        public string GetPropName(int propNum)
        {
            var prop = Properties.GetProperty(propNum);
            return prop.Name;
        }

        public virtual int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        public virtual MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        public virtual void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _methods.GetMethod(methodNumber)((T)this, arguments);
        }

        public virtual void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            retValue = _methods.GetMethod(methodNumber)((T)this, arguments);
        }

        #endregion

        #region IAttachableContext members

        public virtual void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods)
        {
            variables = new IVariable[0];
            methods = GetMethods().ToArray();
        }
        
        public virtual IEnumerable<MethodInfo> GetMethods()
        {
            var array = new MethodInfo[_methods.Count];
            for (int i = 0; i < _methods.Count; i++)
            {
                array[i] = _methods.GetMethodInfo(i);
            }

            return array;
        }

        #endregion
    }
}
