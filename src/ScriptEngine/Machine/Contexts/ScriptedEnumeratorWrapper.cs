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
using ScriptEngine.Machine.Interfaces;

namespace ScriptEngine.Machine.Contexts
{
    public sealed class ScriptedEnumeratorWrapper : IEnumerator<BslValue>
    {
        private readonly UserScriptContextInstance _userObject;

        private BslScriptMethodInfo _moveNextMethod;
        private BslScriptMethodInfo _getCurrentMethod;
        private BslScriptMethodInfo _onDisposeMethod;
        
        public ScriptedEnumeratorWrapper(UserScriptContextInstance userObject)
        {
            _userObject = userObject;
            CheckAndSetMethods();
        }

        private void CheckAndSetMethods()
        {
            var bslInterface = _userObject.Module.GetInterface<IteratorBslInterface>() 
                               ?? IteratorBslInterfaceChecker.CheckModule(_userObject.Module);

            _moveNextMethod = bslInterface.MoveNextMethod;
            _getCurrentMethod = bslInterface.GetCurrentMethod;
            _onDisposeMethod = bslInterface.OnDisposeMethod;
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
    }
}