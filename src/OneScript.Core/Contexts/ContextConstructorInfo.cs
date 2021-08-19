/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Reflection;
using OneScript.Commons;
using OneScript.Values;

namespace OneScript.Contexts
{
    public class ContextConstructorInfo : BslConstructorInfo, IObjectWrapper
    {
        private readonly MethodInfo _factoryMethod;
        
        internal ContextConstructorInfo(MethodInfo staticConstructor)
        {
            CheckMethod(staticConstructor);
            _factoryMethod = staticConstructor;
        }

        private void CheckMethod(MethodInfo staticConstructor)
        {
            if (!staticConstructor.IsStatic)
                throw new ArgumentException("Method must be static");

            if (!typeof(BslObjectValue).IsAssignableFrom(staticConstructor.ReturnType))
                throw new ArgumentException("Return type must inherit BslObjectValue");
        }

        public object UnderlyingObject => _factoryMethod;
        public override Type DeclaringType => _factoryMethod.DeclaringType;
        public override string Name => ConstructorName;
        public override Type ReflectedType => _factoryMethod.DeclaringType;
        public override ParameterInfo[] GetParameters()
        {
            return _factoryMethod.GetParameters();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return _factoryMethod.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return _factoryMethod.Invoke(null, invokeAttr, binder, parameters, culture);
        }
    }
}