/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;

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

        int FindMethod(string name);
        int GetMethodsCount();

        BslMethodInfo GetRuntimeMethodInfo(int methodNumber);
        
        [Obsolete]
        VariableInfo GetPropertyInfo(int propertyNumber);
        void CallAsProcedure(int methodNumber, IValue[] arguments);
        void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue);
    }

    [Obsolete]
    public static class RCIHelperExtensions
    {
        public static IEnumerable<BslMethodInfo> GetMethods(this IRuntimeContextInstance context)
        {
            var methods = new BslMethodInfo[context.GetMethodsCount()];
            for (int i = 0; i < methods.Length; i++)
            {
                methods[i] = context.GetRuntimeMethodInfo(i);
            }

            return methods;
        }

        public static IEnumerable<VariableInfo> GetProperties(this IRuntimeContextInstance context)
        {
            VariableInfo[] infos = new VariableInfo[context.GetPropCount()];
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i] = context.GetPropertyInfo(i);
                // new VariableInfo()
                // {
                //     Identifier = context.GetPropName(i),
                //     Type = SymbolType.ContextProperty,
                //     Index = i
                // };
            }

            return infos;
        }

        public static IValue GetPropValue(this IRuntimeContextInstance context, string propName)
        {
            int propNum = context.FindProperty(propName);

            if (propNum == -1)
            {
                throw RuntimeException.InvalidArgumentValue(propName);
            }

            return context.GetPropValue(propNum);
        }
        
        public static void SetPropValue(this IRuntimeContextInstance context, string propName, IValue value)
        {
            int propNum = context.FindProperty(propName);

            if (propNum == -1)
            {
                throw RuntimeException.InvalidArgumentValue(propName);
            }

            context.SetPropValue(propNum, value);
        }
    }

}