/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Values;
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
        private readonly Func<IBslProcess, IValue[], IValue> _action;
        private const string MethodName_Ru = "Выполнить";
        private const string MethodName_En = "Execute";

        private static BslMethodInfo ExecuteMethodInfo;

        static DelegateAction()
        {
            var builder = BslMethodBuilder.Create()
                .DeclaringType(typeof(DelegateAction))
                .ReturnType(typeof(BslValue))
                .SetNames(MethodName_Ru, MethodName_En);

            ExecuteMethodInfo = builder.Build();
        }
        
        public DelegateAction(Func<IBslProcess, IValue[], IValue> action)
        {
            _action = action;
        }

        public DelegateAction(Func<IBslProcess, BslValue[], BslValue> action)
        {
            _action = (process, parameters) => action(process, parameters
                .Select(x => x is IValueReference r ? r.BslValue : (BslValue)x)
                .ToArray()
            );
        }
        
        public override bool DynamicMethodSignatures => true;

        public override int GetMethodNumber(string name)
        {
            if (string.Compare(name, MethodName_En, StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(name, MethodName_Ru, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return 0;
            }

            return base.GetMethodNumber(name);
        }

        public override int GetMethodsCount()
        {
            return 1;
        }

        public override BslMethodInfo GetMethodInfo(int methodNumber)
        {
            return ExecuteMethodInfo;
        }
        
        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue, IBslProcess process)
        {
            retValue = _action(process, arguments);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments, IBslProcess process)
        {
            _action(process, arguments);
        }

        [ScriptConstructor]
        public static DelegateAction Create(IRuntimeContextInstance target, string methodName)
        {
            var method = target.GetMethodNumber(methodName);

            Func<IBslProcess, IValue[], IValue> action = (process, parameters) =>
            {
                target.CallAsFunction(method, parameters, out var retVal, process);
                return retVal;
            };
            
            return new DelegateAction(action);
        }
    }
}