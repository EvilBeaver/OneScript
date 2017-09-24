/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Environment;
using System;
using System.Reflection;

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
            throw new NotImplementedException();
        }

        private void ReflectVariables(LoadedModule module)
        {
            throw new NotImplementedException();
        }

        public override string Name => _typeName;

        private Type BuildType(string asTypeName)
        {
            throw new NotImplementedException();
        }

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