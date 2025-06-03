/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScriptEngine.Machine.Contexts;

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
        MethodInfo GetMethodInfo(int methodNumber);
        void CallAsProcedure(int methodNumber, IValue[] arguments);
        void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue);

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

        public static IEnumerable<VariableInfo> GetProperties(this IRuntimeContextInstance context, bool withPrivate = false)
        {
            return withPrivate ? GetPropertiesWithPrivate(context) : GetPropertiesWithoutPrivate(context);
        }

        private static IEnumerable<VariableInfo> GetPropertiesWithPrivate(IRuntimeContextInstance context)
        {
            if (!(context is UserScriptContextInstance userScript))
                return GetPropertiesWithoutPrivate(context);

            var infos = new List<VariableInfo>();
            for (int i = 1; i < userScript.GetOwnPropCount(); i++) // skip ThisObject == _ownProperties[0]
            {
                infos.Add(new VariableInfo() { 
                    Identifier = userScript.GetPropName(i),
                    Type = SymbolType.ContextProperty,
                    Index = i,
                    Annotations = Array.Empty<AnnotationDefinition>(),
                    IsExport = false
                });
            }

            return infos.Concat(userScript.Module.Variables);
        }

        private static IEnumerable<VariableInfo> GetPropertiesWithoutPrivate(IRuntimeContextInstance context)
        {
            if (context is UserScriptContextInstance userScript)
                return userScript.Module.Variables.Where(x => x.IsExport).ToList();

            VariableInfo[] infos = new VariableInfo[context.GetPropCount()];
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i] = new VariableInfo()
                {
                    Identifier = context.GetPropName(i),
                    Type = SymbolType.ContextProperty,
                    Index = i,
                    Annotations = Array.Empty<AnnotationDefinition>(),
                    IsExport = true
                };
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
    }

}