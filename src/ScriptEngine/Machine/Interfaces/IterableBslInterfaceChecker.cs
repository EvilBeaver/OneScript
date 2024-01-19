/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Localization;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Interfaces
{
    internal class IterableBslInterfaceChecker : IPredefinedInterfaceChecker
    {
        public static readonly BilingualString IterableAnnotation = new BilingualString("Обходимое", "Iterable"); 
        
        public IEnumerable<PredefinedInterfaceRegistration> GetRegistrations()
        {
            return new[]
            {
                PredefinedInterfaceRegistration.OnMethod(IterableAnnotation, UserScriptContextInstance.OnInstanceCreationTerms),
                PredefinedInterfaceRegistration.OnModule(IterableAnnotation)
            };
        }

        public void Validate(IExecutableModule module)
        {
            var error = new BilingualString(
                "Модуль объекта Обходимое должен содержать функцию " + UserIterableContextInstance.GetIteratorTerms.Russian + "()",
                "Module of Iterable must contain function " + UserIterableContextInstance.GetIteratorTerms.English + "()");

            BslScriptMethodInfo getIteratorMethod;
            
            try
            {
                getIteratorMethod = (BslScriptMethodInfo)module.Methods.Single(method =>
                    UserIterableContextInstance.GetIteratorTerms.HasName(method.Name));
            }
            catch (InvalidOperationException e)
            {
                throw new InterfaceCheckException(error, e);
            }

            if (!getIteratorMethod.IsFunction() || getIteratorMethod.GetBslParameters().Length != 0)
            {
                throw new InterfaceCheckException(error);
            }
            
            BslScriptMethodInfo getCountMethod = (BslScriptMethodInfo)module.Methods.FirstOrDefault(method =>
                UserIterableContextInstance.GetCountTerms.HasName(method.Name));

            if (getCountMethod != null)
            {
                var countError = new BilingualString(
                    "Метод " + UserIterableContextInstance.GetCountTerms.Russian + " должен быть функцией без параметров",
                    "Method " + UserIterableContextInstance.GetCountTerms.English + "() must be a function without parameters");

                if (!getCountMethod.IsFunction() || getCountMethod.GetBslParameters().Length != 0)
                {
                    throw new InterfaceCheckException(countError);
                }
            }

            module.AddInterface(new IterableBslInterface(getIteratorMethod, getCountMethod));
        }
    }
}