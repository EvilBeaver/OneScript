/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class GlobalContextBase<T> : IAttachableContext where T : GlobalContextBase<T>
    {
        protected ContextMethodsMapper<T> Methods { get; } = new ContextMethodsMapper<T>();

        protected ContextPropertyMapper<T> Properties { get; } = new ContextPropertyMapper<T>();

        #region IRuntimeContextInstance members

        public bool IsIndexed
        {
            get { return false; }
        }

        public bool DynamicMethodSignatures => false;

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
            return Properties.FindProperty(name);
        }

        public virtual bool IsPropReadable(int propNum)
        {
            return Properties.GetProperty(propNum).CanRead;
        }

        public virtual bool IsPropWritable(int propNum)
        {
            return Properties.GetProperty(propNum).CanWrite;
        }

        public virtual IValue GetPropValue(int propNum)
        {
            try
            {
                return Properties.GetProperty(propNum).Getter((T)this);
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
                Properties.GetProperty(propNum).Setter((T)this, newVal);
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
            return Methods.FindMethod(name);
        }

        public virtual MethodInfo GetMethodInfo(int methodNumber)
        {
            return Methods.GetMethodInfo(methodNumber);
        }

        public virtual void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            Methods.GetMethod(methodNumber)((T)this, arguments);
        }

        public virtual void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            retValue = Methods.GetMethod(methodNumber)((T)this, arguments);
        }

        #endregion

        #region IAttachableContext members

        public virtual void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods)
        {
            variables = this.GetProperties()
                .OrderBy(x => x.Index)
                .Select(x => Variable.CreateContextPropertyReference(this, x.Index, x.Identifier))
                .ToArray();

            methods = this.GetMethods().ToArray();
        }
        
        public virtual int GetMethodsCount()
        {
            return Methods.Count;
        }

        #endregion
    }
}
