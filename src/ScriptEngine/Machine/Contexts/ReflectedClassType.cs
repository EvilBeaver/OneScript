/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Environment;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.Machine.Contexts
{
    public class ReflectedClassType : TypeDelegator
    {
        private readonly string _typeName;
        private PropertyInfo[] _properties;
        private System.Reflection.MethodInfo[] _methods;
        private FieldInfo[] _fields;
        
        private ReflectedClassType(LoadedModule module, string typeName)
        {
            _typeName = typeName;

            ReflectVariables(module);
            ReflectMethods(module);
        }

        private void ReflectMethods(LoadedModule module)
        {
            _methods = new System.Reflection.MethodInfo[module.Methods.Length];
            for (int i = 0; i < _methods.Length; i++)
            {
                var reflected = CreateMethodInfo(module.Methods[i].Signature);
                reflected.SetDispId(i);
                _methods[i] = reflected;
            }
        }

        private void ReflectVariables(LoadedModule module)
        {
            _properties = new PropertyInfo[module.ExportedProperies.Length];
            for (int i = 0; i < module.ExportedProperies.Length; i++)
            {
                var reflected = CreatePropInfo(module.ExportedProperies[i]);
                _properties[i] = reflected;
            }
        }

        private PropertyInfo CreatePropInfo(ExportedSymbol prop)
        {
            var pi = new ReflectedPropertyInfo(prop.SymbolicName);
            pi.SetDispId(prop.Index);
            return pi;
        }

        private ReflectedMethodInfo CreateMethodInfo(ScriptEngine.Machine.MethodInfo methInfo)
        {
            var reflectedMethod = new ReflectedMethodInfo(methInfo.Name);
            reflectedMethod.IsFunction = methInfo.IsFunction;
            for (int i = 0; i < methInfo.Params.Length; i++)
            {
                var currentParam = methInfo.Params[i];
                var reflectedParam = new ReflectedParamInfo("param" + i.ToString(), currentParam.IsByValue);
                reflectedParam.SetOwner(reflectedMethod);
                reflectedParam.SetPosition(i);
                reflectedMethod.Parameters.Add(reflectedParam);
            }

            return reflectedMethod;

        }

        public override string Name => _typeName;

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return new FieldInfo[0];
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return null;
        }

        protected override System.Reflection.MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return _methods.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.Name, name) == 0);
        }

        public override System.Reflection.MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return _methods;
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return _properties;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return _properties.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.Name, name) == 0);
        }

        /////////////////////////////////////////////////////////////////////////////////////

        public static Type ReflectModule(LoadedModuleHandle module, string asTypeName)
        {
            return ReflectModule(module.Module, asTypeName);
        }

        internal static Type ReflectModule(LoadedModule module, string asTypeName)
        {
            return new ReflectedClassType(module, asTypeName);
        }
        
    }
}