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
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine.Interfaces;

namespace ScriptEngine.Machine.Contexts
{
    public class UserIterableContextInstance : UserScriptContextInstance, ICollectionContext<BslValue>
    {
        private BslScriptMethodInfo _getCountMethod;
        private BslScriptMethodInfo _getIteratorMethod;
        public static readonly BilingualString GetIteratorTerms = new BilingualString("ПолучитьИтератор", "GetIterator");
        public static readonly BilingualString GetCountTerms = new BilingualString("Количество", "Count");

        public UserIterableContextInstance(IExecutableModule module, TypeDescriptor asObjectOfType, IValue[] args = null) : base(module, asObjectOfType, args)
        {
            var methods = module.GetInterface<IterableBslInterface>();
            _getIteratorMethod = methods.GetIteratorMethod;
            _getCountMethod = methods.GetCountMethod;
        }

        public int Count()
        {
            if (_getCountMethod == null)
                throw new RuntimeException(
                    new BilingualString(
                        "Класс не поддерживает получение количества элементов, т.к. в нем отсутствует метод "+GetCountTerms.Russian+"()",
                        "Class doesn't support items counting, because method "+GetCountTerms.English+"() is not defined")
                );

            CallAsFunction(_getCountMethod.DispatchId, Array.Empty<IValue>(), out var ret);

            return (int)ret.AsNumber();
        }

        public IEnumerator<BslValue> GetEnumerator()
        {
            CallAsFunction(_getIteratorMethod.DispatchId, Array.Empty<IValue>(), out var enumerator);
            if (!(enumerator is UserScriptContextInstance userObject))
            {
                throw ScriptedEnumeratorWrapper.IncompatibleInterfaceError();
            }

            return new ScriptedEnumeratorWrapper(userObject);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}