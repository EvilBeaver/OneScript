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
    public abstract class AutoContext<TInstance> : LibraryContextBase where TInstance : AutoContext<TInstance>
    {
        public override bool IsPropReadable(int propNum)
        {
            return _properties.GetProperty(propNum).CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            return _properties.GetProperty(propNum).CanWrite;
        }

        public override int FindProperty(string name)
        {
            return _properties.FindProperty(name);
        }

        public override IValue GetPropValue(int propNum)
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

        public override void SetPropValue(int propNum, IValue newVal)
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

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
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

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            try
            {
                retValue = _methods.GetMethod(methodNumber)((TInstance)this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        public override IEnumerable<VariableInfo> GetProperties()
        {
            var allProps = _properties.GetProperties();
            var result = new VariableInfo[allProps.Length];
            for (int i = 0; i < allProps.Length; i++)
            {
                result[i] = new VariableInfo()
                {
                    Identifier = allProps[i],
                    Index = i,
                    Type = SymbolType.ContextProperty
                };
            }

            return result;

        }

        private static ContextPropertyMapper<TInstance> _properties = new ContextPropertyMapper<TInstance>();
        private static ContextMethodsMapper<TInstance> _methods = new ContextMethodsMapper<TInstance>();
    }
}
