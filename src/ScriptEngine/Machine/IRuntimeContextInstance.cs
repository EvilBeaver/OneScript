/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public interface IRuntimeContextInstance
    {
        bool IsIndexed { get; }
        bool DynamicMethodSignatures { get; }

        IValue GetIndexedValue(IValue index);
        void SetIndexedValue(IValue index, IValue val);

        int FindProperty(string name);
        bool IsPropReadable(int propNum);
        bool IsPropWritable(int propNum);
        IValue GetPropValue(int propNum);
        void SetPropValue(int propNum, IValue newVal);

        int GetPropCount();
        string GetPropName(int propNum);

        IEnumerable<MethodInfo> GetMethods();
        int FindMethod(string name);
        MethodInfo GetMethodInfo(int methodNumber);
        void CallAsProcedure(int methodNumber, IValue[] arguments);
        void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue);

    }

    public static class RCIHelperExtensions
    {
        // TODO: kill GetMethods in IRuntimeContextInstance
        public static IEnumerable<MethodInfo> _GetMethods(this IRuntimeContextInstance context)
        {
            return context.GetMethods();
        }

        public static IEnumerable<VariableInfo> GetProperties(this IRuntimeContextInstance context)
        {
            VariableInfo[] infos = new VariableInfo[context.GetPropCount()];
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i] = new VariableInfo()
                {
                    Identifier = context.GetPropName(i),
                    Type = SymbolType.ContextProperty,
                    Index = i
                };
            }

            return infos;
        }
    }

}