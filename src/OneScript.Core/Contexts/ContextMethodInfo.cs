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
using OneScript.Commons;
using OneScript.Execution;

namespace OneScript.Contexts
{
    /// <summary>
    /// Информация о методе, объявленном в классе .NET и помеченном атрибутом ContextMethod
    /// </summary>
    public sealed class ContextMethodInfo : BslMethodInfo, IObjectWrapper, ISupportsDeprecation
    {
        private readonly MethodInfo _realMethod;

        public ContextMethodInfo(MethodInfo realMethod)
            : this(realMethod, realMethod.GetCustomAttribute<ContextMethodAttribute>(false)
                               ?? throw new ArgumentException("Method is not marked with ContextMethodAttribute"))
        {
        }

        public ContextMethodInfo(MethodInfo realMethod, ContextMethodAttribute binding)
        {
            _realMethod = realMethod;
            InjectsProcess = _realMethod.GetParameters().FirstOrDefault()?.ParameterType == typeof(IBslProcess);
            IsDeprecated = binding.IsDeprecated;
            IsForbiddenToUse = binding.ThrowOnUse;
            Name = binding.Name;
            Alias = binding.Alias;
        }

        public override Type ReturnType => _realMethod.ReturnType;

        public override ParameterInfo ReturnParameter => _realMethod.ReturnParameter;

        public bool IsDeprecated { get; }

        public bool IsForbiddenToUse { get; }
        
        public override object[] GetCustomAttributes(bool inherit)
        {
            return _realMethod.GetCustomAttributes(inherit);
        }
        
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _realMethod.GetCustomAttributes(attributeType, inherit);
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

        public override MethodInfo GetBaseDefinition()
        {
            return _realMethod.GetBaseDefinition();
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => _realMethod.ReturnTypeCustomAttributes;

        public override string Name { get; }

        public override string Alias { get; }

        public override Type DeclaringType => _realMethod.DeclaringType;

        public override Type ReflectedType => _realMethod.ReflectedType;
        
        public override RuntimeMethodHandle MethodHandle => _realMethod.MethodHandle;

        public override MethodAttributes Attributes => _realMethod.Attributes;


        public object UnderlyingObject => _realMethod;

        public bool InjectsProcess { get; }

        public MethodInfo GetWrappedMethod() => _realMethod;
    }
}
