/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public class ReflectedClassType<T> : TypeDelegator where T : ScriptDrivenObject
    {
        private string _typeName;
        private PropertyInfo[] _properties;
        private System.Reflection.MethodInfo[] _methods;
        private FieldInfo[] _fields;
        private ConstructorInfo[] _constructors;
        
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
            _fields = source.Select(x =>
            {
                if (x is ReflectedFieldInfo refl)
                {
                    refl.SetDeclaringType(this);
                }

                return x;
            }).ToArray();
        }

        public void SetProperties(IEnumerable<PropertyInfo> source)
        {
            _properties = source.ToArray();
        }

        public void SetMethods(IEnumerable<System.Reflection.MethodInfo> source)
        {
            _methods = source.Select(x =>
            {
                if (x is ReflectedMethodInfo refl)
                {
                    refl.SetDeclaringType(this);
                }

                return x;
            }).ToArray();
        }

        public void SetConstructors(IEnumerable<System.Reflection.ConstructorInfo> source)
        {
            _constructors = source.Select(x =>
            {
                if(x is ReflectedConstructorInfo refl)
                    refl.SetDeclaringType(this);
                return x;

            }).ToArray();
        }

        public override string Name => _typeName;
        public override string FullName => Namespace + "." + Name;
        public override Assembly Assembly => Assembly.GetExecutingAssembly();
        public override bool ContainsGenericParameters => false;
        public override string AssemblyQualifiedName => Assembly.CreateQualifiedName(Assembly.FullName, Name);
        public override Type UnderlyingSystemType => typeof(T);
        public override Type BaseType => typeof(T).BaseType;
        public override IEnumerable<CustomAttributeData> CustomAttributes => null;
        public override string Namespace => GetType().Namespace + ".dyn";

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            IEnumerable<FieldInfo> result;
            bool showPublic = bindingAttr.HasFlag(BindingFlags.Public);
            bool showPrivate = bindingAttr.HasFlag(BindingFlags.NonPublic);
            if (showPublic && showPrivate)
                result = _fields;
            else
                result = _fields.Where(x => x.IsPublic && showPublic || x.IsPrivate && showPrivate);
            
            return result.ToArray();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            if (bindingAttr.HasFlag(BindingFlags.Public))
            {
                return _fields.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.Name, name) == 0 || x.IsPublic);
            }

            return _fields.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.Name, name) == 0);
        }

        protected override System.Reflection.MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return _methods.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Compare(x.Name, name) == 0);
        }

        public override System.Reflection.MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            bool showPrivate = bindingAttr.HasFlag(BindingFlags.NonPublic);
            bool showPublic = bindingAttr.HasFlag(BindingFlags.Public);
            return _methods.Where(x=>x.IsPublic && showPublic || x.IsPrivate && showPrivate).ToArray();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            bool showPrivate = bindingAttr.HasFlag(BindingFlags.NonPublic);
            bool showPublic = bindingAttr.HasFlag(BindingFlags.Public);

            return _properties.Where(x =>
            {
                var isPublic = (x.GetMethod?.IsPublic) == true || (x.SetMethod?.IsPublic) == true;
                return isPublic && showPublic || !isPublic && showPrivate;

            }).ToArray();
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
            if(name == null)
                throw new ArgumentNullException();

            switch (type)
            {
                case MemberTypes.Field:
                    return new MemberInfo[] { GetField(name, bindingAttr) };
                case MemberTypes.Method:
                    return new MemberInfo[] { GetMethod(name, bindingAttr) };
                case MemberTypes.Property:
                    return new MemberInfo[] { GetProperty(name, bindingAttr) };
                default:
                    return new MemberInfo[0];
            }
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return _constructors;
        }
        
    }
}