/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Localization;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public class ScriptedEnumeratorWrapper : IEnumerator<BslValue>
    {
        private readonly UserScriptContextInstance _userObject;

        private static readonly BilingualString
            MoveNextMethodName = new BilingualString("ВыбратьСледующий", "MoveNext");
        
        private static readonly BilingualString
            GetCurrentMethodName = new BilingualString("ТекущийЭлемент", "CurrentItem");
        
        private static readonly BilingualString
            DisposeMethodName = new BilingualString("ПриОсвобожденииОбъекта", "OnDispose");

        private BslScriptMethodInfo _moveNextMethod;
        private BslScriptMethodInfo _getCurrentMethod;
        private BslScriptMethodInfo _onDisposeMethod;
        
        public ScriptedEnumeratorWrapper(UserScriptContextInstance userObject)
        {
            _userObject = userObject;
            CheckCompatibility();
        }

        private void CheckCompatibility()
        {
            _moveNextMethod = FindRequiredMethod(MoveNextMethodName);
            _getCurrentMethod = FindRequiredMethod(GetCurrentMethodName);
            _onDisposeMethod = FindOptionalMethod(DisposeMethodName);

            if (!_moveNextMethod.IsFunction() || _moveNextMethod.GetBslParameters().Length != 0)
            {
                throw MissingMethod(MoveNextMethodName);
            }
            
            if (!_getCurrentMethod.IsFunction() || _getCurrentMethod.GetBslParameters().Length != 0)
            {
                throw MissingMethod(GetCurrentMethodName);
            }

            if (_onDisposeMethod != null && _onDisposeMethod.GetBslParameters().Length != 0)
            {
                throw MissingMethod(DisposeMethodName);
            }
        }

        private BslScriptMethodInfo FindRequiredMethod(BilingualString names)
        {
            try
            {
                return (BslScriptMethodInfo)_userObject.Module.Methods.Single(m =>
                    names.HasName(m.Name, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (InvalidOperationException e)
            {
                throw MissingMethod(names, e);
            }
        }

        private BslScriptMethodInfo FindOptionalMethod(BilingualString names)
        {
            return (BslScriptMethodInfo)_userObject.Module.Methods.FirstOrDefault(m =>
                names.HasName(m.Name, StringComparison.CurrentCultureIgnoreCase));
            
        }

        public bool MoveNext()
        {
            _userObject.CallAsFunction(_moveNextMethod.DispatchId, Array.Empty<IValue>(), out var result);
            return result.AsBoolean();
        }

        public void Reset()
        {
            throw new System.NotSupportedException();
        }

        public BslValue Current
        {
            get
            {
                _userObject.CallAsFunction(_getCurrentMethod.DispatchId, Array.Empty<IValue>(), out var result);
                return (BslValue)result;
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            if (_onDisposeMethod != null)
                _userObject.CallAsProcedure(_onDisposeMethod.DispatchId, Array.Empty<IValue>());
        }

        public static RuntimeException IncompatibleInterfaceError()
        {
            var error = new BilingualString(
                "Итератор не соответствует интерфейсу итератора",
                "Iterator doesn't match Iterator interface");

            return new RuntimeException(error);
        }

        public static RuntimeException MissingMethod(BilingualString methodName, Exception parent = null)
        {
            var error = new BilingualString(
                "Обязательный метод "+methodName.Russian+"отсутствует, или не соответствует интерфейсу итератора",
                "Required method "+methodName.English+" is missing or doesn't match iterator interface");

            return new RuntimeException(error, parent);
        }
    }
}