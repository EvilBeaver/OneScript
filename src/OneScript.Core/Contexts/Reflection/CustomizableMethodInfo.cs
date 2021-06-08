/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace OneScript.Contexts.Reflection
{
    public abstract class CustomizableMethodInfo : BslMethodInfoBase
    {
        private Type _declaringType;
        private Type _returnType;
        private string _name;
        private bool _isPrivate = true;
        
        public override Type ReturnType => _returnType;
        public override Type DeclaringType => _declaringType;
        public override string Name => _name;
        public override Type ReflectedType => _declaringType;

        public List<BslParameterInfo> Parameters { get; } = new List<BslParameterInfo>();
        public override MethodImplAttributes GetMethodImplementationFlags() => MethodImplAttributes.Managed;
        public override ParameterInfo[] GetParameters() => Parameters.ToArray();
        
        protected bool IsPrivate => _isPrivate;
        
        public void SetDeclaringType(Type type)
        {
            _declaringType = type;
        }

        public void SetName(string name)
        {
            _name = name;
        }
        
        public void SetAlias(string alias)
        {
            Alias = alias;
        }

        public void SetNames(string name, string alias)
        {
            SetName(name);
            SetAlias(alias);
        }
        
        public void SetReturnType(Type type)
        {
            _returnType = type;
        }
        
        public void SetPrivate(bool makePrivate)
        {
            _isPrivate = makePrivate;
        }
        
        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override MethodAttributes Attributes => IsPrivate ? MethodAttributes.Private : MethodAttributes.Public;

        public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();
        
        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();
    }
}