/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                return Array.Empty<VariableInfo>();

            List<VariableInfo> infos = new List<VariableInfo>();
            foreach (var variable in userScript.Module.Variables)
            {
                infos.Add(new VariableInfo() { 
                    Identifier = variable.Identifier,
                    Type = variable.Type,
                    Index = variable.Index,
                    Annotations = HackGetAnnotations(context, variable.Index),
                    IsExport = variable.IsExport
                });
            }

            return infos;
        }

        private static IEnumerable<VariableInfo> GetPropertiesWithoutPrivate(IRuntimeContextInstance context)
        {
            VariableInfo[] infos = new VariableInfo[context.GetPropCount()];
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i] = new VariableInfo()
                {
                    Identifier = context.GetPropName(i),
                    Type = SymbolType.ContextProperty,
                    Index = i,
                    Annotations = HackGetAnnotations(context, i),
                    IsExport = true
                };
            }

            return infos;
        }

        private static AnnotationDefinition[] HackGetAnnotations(IRuntimeContextInstance context, int i)
        {
            if (!(context is UserScriptContextInstance userScript)) 
                return Array.Empty<AnnotationDefinition>();
            
            if (i == 0)
                return Array.Empty<AnnotationDefinition>();
            
            var variable = userScript.Module.Variables[i - 1];
            return variable.Annotations;
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