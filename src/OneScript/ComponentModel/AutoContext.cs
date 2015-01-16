using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Core;

namespace OneScript.ComponentModel
{
    public abstract class AutoContext<TInstance> : ContextBase where TInstance : AutoContext<TInstance>
    {
        public override bool IsPropReadable(int propNum)
        {
            return _properties.GetProperty(propNum).CanRead;
        }

        public override bool IsPropWriteable(int propNum)
        {
            return _properties.GetProperty(propNum).CanWrite;
        }

        public override int FindProperty(string name)
        {
            return _properties.FindProperty(name);
        }

        public override int GetPropCount()
        {
            return _properties.Count;
        }

        public override string GetPropertyName(int index, NameRetrievalMode mode = NameRetrievalMode.Default)
        {
            var def = _properties.GetProperty(index);

            switch(mode)
            {
                case NameRetrievalMode.PreferAlias:
                    if (def.Alias != null)
                        return def.Alias;
                    else
                        return def.Name;
                case NameRetrievalMode.OnlyAlias:
                    return def.Alias;
                default:
                    return def.Name;
            }
        }

        protected override IValue GetPropertyValueInternal(int propNum)
        {
            try
            {
                return _properties.GetProperty(propNum).Getter((TInstance)this);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        protected override void SetPropertyValueInternal(int propNum, IValue newVal)
        {
            try
            {
                _properties.GetProperty(propNum).Setter((TInstance)this, newVal);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        public override int GetMethodsCount()
        {
            return _methods.Count;
        }

        public override string GetMethodName(int index, NameRetrievalMode mode = NameRetrievalMode.Default)
        {
            return _methods.GetMethodName(index, mode);
        }

        public override int GetParametersCount(int index)
        {
            return _methods.GetParametersCount(index);
        }

        public override bool HasReturnValue(int index)
        {
            return _methods.HasReturnValue(index);
        }

        public override bool GetDefaultValue(int methodIndex, int paramIndex, out IValue defaultValue)
        {
            return _methods.GetDefaultValue(methodIndex, paramIndex, out defaultValue);
        }
        
        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            try
            {
                _methods.GetMethod(methodNumber)((TInstance)this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override IValue CallAsFunction(int methodNumber, IValue[] arguments)
        {
            try
            {
                return _methods.GetMethod(methodNumber)((TInstance)this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        } 

        private static ContextPropertyMapper<TInstance> _properties = new ContextPropertyMapper<TInstance>();
        private static ContextMethodsMapper<TInstance> _methods = new ContextMethodsMapper<TInstance>();
    }
}
