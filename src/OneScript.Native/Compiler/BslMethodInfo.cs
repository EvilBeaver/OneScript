/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace OneScript.Native.Compiler
{
    public class BslMethodInfo : MethodInfo
    {
        public BslMethodInfo(LambdaExpression implementation)
        {
            throw new NotImplementedException();
        }
        
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override Type DeclaringType { get; }
        public override string Name { get; }
        public override Type ReflectedType { get; }
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
            throw new NotImplementedException();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes { get; }
    }
}