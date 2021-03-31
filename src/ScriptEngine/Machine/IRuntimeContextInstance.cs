/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Core;

namespace ScriptEngine.Machine
{
    public interface IRuntimeContextInstance : IContext
    {
        MethodInfo GetMethodInfo(int methodNumber);
        VariableInfo GetPropertyInfo(int propertyNumber);
        
    }

    public static class RCIHelperExtensions
    {
        public static IEnumerable<MethodInfo> GetMethods(this IRuntimeContextInstance context)
        {
            MethodInfo[] methods = new MethodInfo[context.GetMethodsCount()];
            for (int i = 0; i < methods.Length; i++)
            {
                methods[i] = context.GetMethodInfo(i);
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