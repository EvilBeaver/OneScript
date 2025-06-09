/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Execution;
using ScriptEngine.Machine;

namespace OneScript.Contexts
{
    public interface IRuntimeContextInstance
    {
        bool IsIndexed { get; }
        bool DynamicMethodSignatures { get; }

        IValue GetIndexedValue(IValue index);
        void SetIndexedValue(IValue index, IValue val);

        int GetPropertyNumber(string name);
        bool IsPropReadable(int propNum);
        bool IsPropWritable(int propNum);
        IValue GetPropValue(int propNum);
        void SetPropValue(int propNum, IValue newVal);

        int GetPropCount();
        string GetPropName(int propNum);

        int GetMethodNumber(string name);
        int GetMethodsCount();

        BslMethodInfo GetMethodInfo(int methodNumber);

        BslPropertyInfo GetPropertyInfo(int propertyNumber);

        void CallAsProcedure(int methodNumber, IValue[] arguments, IBslProcess process);

        void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue, IBslProcess process);

        [Obsolete("Use function with IBslProcess parameter")]
        void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            CallAsProcedure(methodNumber, arguments, ForbiddenBslProcess.Instance);
        }

        [Obsolete("Use function with IBslProcess parameter")]
        void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            CallAsFunction(methodNumber, arguments, out retValue, ForbiddenBslProcess.Instance);
        }
    }
}