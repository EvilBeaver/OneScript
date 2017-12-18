/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class AutoContext<TInstance> : PropertyNameIndexAccessor where TInstance : AutoContext<TInstance>
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
                Debug.Assert(e.InnerException != null);
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

        public override int GetPropCount()
        {
            return _properties.Count;
        }

        public override string GetPropName(int propNum)
        {
            return _properties.GetProperty(propNum).Name;
        }

        public override int FindMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        public override int GetMethodsCount()
        {
            return _methods.Count;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return _methods.GetMethodInfo(methodNumber);
        }

        private void CheckIfCallIsPossible(int methodNumber, IValue[] arguments)
        {
            var methodInfo = _methods.GetMethodInfo(methodNumber);
            if (!methodInfo.IsDeprecated)
            {
                return;
            }
            if (methodInfo.ThrowOnUseDeprecated)
            {
                throw RuntimeException.DeprecatedMethodCall(methodInfo.Name);
            }
            if (_warnedDeprecatedMethods.Contains(methodNumber))
            {
                return;
            }
            SystemLogger.Write($"ВНИМАНИЕ! Вызов устаревшего метода {methodInfo.Name}");
            _warnedDeprecatedMethods.Add(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            CheckIfCallIsPossible(methodNumber, arguments);
            try
            {
                _methods.GetMethod(methodNumber)((TInstance)this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Debug.Assert(e.InnerException != null);
                throw e.InnerException;
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            CheckIfCallIsPossible(methodNumber, arguments);
            try
            {
                retValue = _methods.GetMethod(methodNumber)((TInstance)this, arguments);
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Debug.Assert(e.InnerException != null);
                throw e.InnerException;
            }
        }

        private static readonly ContextPropertyMapper<TInstance> _properties = new ContextPropertyMapper<TInstance>();
        private static readonly ContextMethodsMapper<TInstance> _methods = new ContextMethodsMapper<TInstance>();
        private static readonly HashSet<int> _warnedDeprecatedMethods = new HashSet<int>();
    }
}
