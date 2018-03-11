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
    public class ReflectedClassType<T> : TypeDelegator where T : ScriptDrivenObject
    {
        private string _typeName;
        private PropertyInfo[] _properties;
        private System.Reflection.MethodInfo[] _methods;
        private FieldInfo[] _fields;
        
        public ReflectedClassType()
            :base(typeof(T))
        {
        }

        public void SetName(string name)
        {
            _typeName = name;
        }

        public void SetFields(IEnumerable<FieldInfo> source)
        {
            _fields = source.ToArray();
        }

        public void SetProperties(IEnumerable<PropertyInfo> source)
        {
            _properties = source.ToArray();
        }

        public void SetMethods(IEnumerable<System.Reflection.MethodInfo> source)
        {
            _methods = source.ToArray();
        }

        public override string Name => _typeName;
        public override string FullName => Namespace + "." + Name;
        public override Assembly Assembly => Assembly.GetExecutingAssembly();
        public override bool ContainsGenericParameters => false;
        public override string AssemblyQualifiedName => Assembly.CreateQualifiedName(Assembly.FullName, Name);
        public override Type UnderlyingSystemType => typeof(ScriptDrivenObject);
        public override Type BaseType => null;
        public override IEnumerable<CustomAttributeData> CustomAttributes => null;
        public override string Namespace => GetType().Namespace + ".dyn";

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

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            var mems = new MemberInfo[_properties.Length + _fields.Length + _methods.Length];

            Array.Copy(_fields, mems, _fields.Length);
            Array.Copy(_properties, 0, mems, _fields.Length, _properties.Length);
            Array.Copy(_methods, 0, mems, _properties.Length + _fields.Length, _methods.Length);

            return mems;
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            switch (type)
            {
                case MemberTypes.Method:
                    return new MemberInfo[] { GetMethod(name) };
                case MemberTypes.Property:
                    return new MemberInfo[] { GetProperty(name) };
                default:
                    return new MemberInfo[0];
            }
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return new ConstructorInfo[0];
        }
        
    }
}