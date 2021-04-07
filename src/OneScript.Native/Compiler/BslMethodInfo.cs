/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OneScript.Native.Compiler
{
    public class BslMethodInfo : MethodInfo
    {
        private Type _declaringType;
        private string _name;
        private object[] _annotations;
        
        public void SetDeclaringType(Type type)
        {
            _declaringType = type;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
        }
        
        public LambdaExpression Implementation { get; private set; }
        
        public override object[] GetCustomAttributes(bool inherit)
        {
            return _annotations ?? new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Where(x => x.GetType() == attributeType).ToArray();
        }
        
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Any(x => x.GetType() == attributeType);
        }

        public override Type DeclaringType => _declaringType;
        public override string Name => _name;

        public override Type ReflectedType => _declaringType;
        
        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            throw new NotImplementedException();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override MethodAttributes Attributes { get; }
        
        public override RuntimeMethodHandle MethodHandle { get; }
        
        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes { get; }
    }
}