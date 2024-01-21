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
    internal class IteratorBslInterfaceChecker : IPredefinedInterfaceChecker
    {
        public static readonly BilingualString IterableAnnotation = new BilingualString("Итератор", "Iterator");
        
        private static readonly BilingualString
            MoveNextMethodName = new BilingualString("Следующий", "Next");
        
        private static readonly BilingualString
            GetCurrentMethodName = new BilingualString("ТекущийЭлемент", "CurrentItem");
        
        private static readonly BilingualString
            DisposeMethodName = new BilingualString("ПриОсвобожденииОбъекта", "OnDispose");
        
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
            CheckModule(module);
        }

        internal static IteratorBslInterface CheckModule(IExecutableModule module)
        {
            var moveNextMethod = FindRequiredMethod(module, MoveNextMethodName);
            var getCurrentMethod = FindRequiredMethod(module, GetCurrentMethodName);
            var onDisposeMethod = FindOptionalMethod(module, DisposeMethodName);

            if (!moveNextMethod.IsFunction() || moveNextMethod.GetBslParameters().Length != 0)
            {
                throw MissingMethod(MoveNextMethodName);
            }
            
            if (!getCurrentMethod.IsFunction() || getCurrentMethod.GetBslParameters().Length != 0)
            {
                throw MissingMethod(GetCurrentMethodName);
            }

            if (onDisposeMethod != null && onDisposeMethod.GetBslParameters().Length != 0)
            {
                throw MissingMethod(DisposeMethodName);
            }

            var bslInterface = new IteratorBslInterface(moveNextMethod, getCurrentMethod, onDisposeMethod);
            module.AddInterface(bslInterface);

            return bslInterface;
        }
        
        private static BslScriptMethodInfo FindRequiredMethod(IExecutableModule module, BilingualString names)
        {
            try
            {
                return (BslScriptMethodInfo)module.Methods.Single(m =>
                    names.HasName(m.Name));
            }
            catch (InvalidOperationException e)
            {
                throw MissingMethod(names, e);
            }
        }

        private static BslScriptMethodInfo FindOptionalMethod(IExecutableModule module,BilingualString names)
        {
            return (BslScriptMethodInfo)module.Methods.FirstOrDefault(m =>
                names.HasName(m.Name));
            
        }
        
        private static InterfaceCheckException MissingMethod(BilingualString methodName, Exception parent = null)
        {
            var error = new BilingualString(
                "Обязательный метод "+methodName.Russian+" отсутствует, или не соответствует интерфейсу итератора",
                "Required method "+methodName.English+" is missing or doesn't match iterator interface");

            return new InterfaceCheckException(error, parent);
        }
    }
}