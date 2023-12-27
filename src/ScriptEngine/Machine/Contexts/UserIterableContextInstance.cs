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

namespace ScriptEngine.Machine.Contexts
{
    public class UserIterableContextInstance : UserScriptContextInstance, ICollectionContext<BslValue>
    {
        private static BilingualString GetIteratorName = new BilingualString("ПолучитьИтератор", "GetIterator");
        private static BilingualString GetCountName = new BilingualString("Количество", "Count");

        private BslScriptMethodInfo _getCountMethod;
        private BslScriptMethodInfo _getIteratorMethod;
        
        public UserIterableContextInstance(IExecutableModule module, TypeDescriptor asObjectOfType, IValue[] args = null) : base(module, asObjectOfType, args)
        {
            CheckModuleCompatibility(module);
        }

        private void CheckModuleCompatibility(IExecutableModule module)
        {
            var error = new BilingualString(
                "Модуль объекта Обходимое должен содержать функцию " + GetIteratorName.Russian + "()",
                "Module of Iterable must contain function " + GetIteratorName.English + "()");
            
            try
            {
                _getIteratorMethod = (BslScriptMethodInfo)module.Methods.Single(method =>
                    GetIteratorName.HasName(method.Name, StringComparison.CurrentCultureIgnoreCase));
            }
            catch (InvalidOperationException e)
            {
                throw new RuntimeException(error, e);
            }

            if (!_getIteratorMethod.IsFunction() || _getIteratorMethod.GetBslParameters().Length != 0)
            {
                throw new RuntimeException(error);
            }
            
            _getCountMethod = (BslScriptMethodInfo)module.Methods.FirstOrDefault(method =>
                GetCountName.HasName(method.Name, StringComparison.CurrentCultureIgnoreCase));

            if (_getCountMethod != null)
            {
                var countError = new BilingualString(
                    "Метод " + GetIteratorName.Russian + " должен быть функцией без параметров",
                    "Method " + GetIteratorName.English + "() must be a function without parameters");

                if (!_getCountMethod.IsFunction() || _getCountMethod.GetBslParameters().Length != 0)
                {
                    throw new RuntimeException(countError);
                }
            }
        }

        public int Count()
        {
            if (_getCountMethod == null)
                throw new RuntimeException(
                    new BilingualString(
                        "Класс не поддерживает получение количества элементов, т.к. в нем отсутствует метод "+GetCountName.Russian+"()",
                        "Class doesn't support items counting, because method "+GetCountName.English+"() is not defined")
                );

            var ret = CallScriptMethod(_getCountMethod.DispatchId, Array.Empty<IValue>());

            return (int)ret.AsNumber();
        }

        public IEnumerator<BslValue> GetEnumerator()
        {
            var enumerator = CallScriptMethod(_getIteratorMethod.DispatchId, Array.Empty<IValue>());
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