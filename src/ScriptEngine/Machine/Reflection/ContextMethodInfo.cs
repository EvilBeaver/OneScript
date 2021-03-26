/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public sealed class ContextMethodInfo : System.Reflection.MethodInfo, IObjectWrapper
    {
        private readonly System.Reflection.MethodInfo _realMethod;
        private readonly ContextMethodAttribute _scriptMark;

        public ContextMethodInfo(System.Reflection.MethodInfo realMethod)
        {
            _realMethod = realMethod;
            _scriptMark = (ContextMethodAttribute)GetCustomAttributes(typeof(ContextMethodAttribute), false).First();
        }

        public override Type ReturnType => _realMethod.ReturnType;

        public override ParameterInfo ReturnParameter => _realMethod.ReturnParameter;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _realMethod.GetCustomAttributes(inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _realMethod.IsDefined(attributeType, inherit);
        }

        public override ParameterInfo[] GetParameters()
        {
            return _realMethod.GetParameters();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return _realMethod.GetMethodImplementationFlags();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return _realMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override System.Reflection.MethodInfo GetBaseDefinition()
        {
            return _realMethod.GetBaseDefinition();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => _realMethod.ReturnTypeCustomAttributes;

        public override string Name => _scriptMark.GetName();

        public override Type DeclaringType => _realMethod.DeclaringType;

        public override Type ReflectedType => _realMethod.ReflectedType;
        
        public override RuntimeMethodHandle MethodHandle => _realMethod.MethodHandle;

        public override MethodAttributes Attributes => _realMethod.Attributes;

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _realMethod.GetCustomAttributes(attributeType, inherit);
        }

        public object UnderlyingObject => _realMethod;

        public System.Reflection.MethodInfo GetWrappedMethod() => _realMethod;
    }
}
