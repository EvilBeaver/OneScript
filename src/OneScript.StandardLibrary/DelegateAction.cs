/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Делегат для выполнения метода в другом объекте
    /// </summary>
    [ContextClass("Действие","Action")]
    public class DelegateAction : ContextIValueImpl
    {
        private readonly Func<IValue[], IValue> _action;
        private const string MethodName_Ru = "Выполнить";
        private const string MethodName_En = "Execute";

        private DelegateAction(Func<IValue[], IValue> action)
        {
            _action = action;
        }

        public override bool DynamicMethodSignatures => true;

        public override int FindMethod(string name)
        {
            if (string.Compare(name, MethodName_En, StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(name, MethodName_Ru, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return 0;
            }

            return base.FindMethod(name);
        }

        public override int GetMethodsCount()
        {
            return 1;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return new MethodInfo
            {
                Name = MethodName_Ru,
                Alias = MethodName_En,
                Annotations = new AnnotationDefinition[0],
                Params = new ParameterDefinition[0]
            };
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            retValue = _action(arguments);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            _action(arguments);
        }

        [ScriptConstructor]
        public static DelegateAction Create(IRuntimeContextInstance target, string methodName)
        {
            var method = target.FindMethod(methodName);

            Func<IValue[], IValue> action = (parameters) =>
            {
                IValue retVal;
                target.CallAsFunction(method, parameters, out retVal);
                return retVal;
            };
            
            return new DelegateAction(action);
        }
    }
}