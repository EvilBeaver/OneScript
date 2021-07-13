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
using OneScript.Types;

namespace OneScript.Contexts
{
    public class ContextPropertyInfo : BslPropertyInfo, IObjectWrapper
    {
        private readonly PropertyInfo _realProperty;
        private readonly ContextPropertyAttribute _scriptMark;
        
        public ContextPropertyInfo(PropertyInfo wrappedInfo)
        {
            _realProperty = wrappedInfo;
            _scriptMark = (ContextPropertyAttribute)_realProperty.GetCustomAttributes(typeof(ContextPropertyAttribute), false).First();
        }
        
        public override object[] GetCustomAttributes(bool inherit)
        {
            return _realProperty.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return _realProperty.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return _realProperty.IsDefined(attributeType, inherit);
        }

        public override Type DeclaringType => _realProperty.DeclaringType;
        public override Type ReflectedType => _realProperty.ReflectedType;
        
        public override string Name => _scriptMark.GetName();
        
        public override string Alias => _scriptMark.GetAlias();
        
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            var getter = GetGetMethod(nonPublic);
            var setter = GetSetMethod(nonPublic);

            if (getter != null && setter != null)
            {
                return new [] {getter, setter};
            }

            return setter == null ? new [] {getter} : new [] {setter};
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return _scriptMark.CanRead ? _realProperty.GetGetMethod(nonPublic) : null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return _scriptMark.CanWrite ? _realProperty.GetSetMethod(nonPublic) : null;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override PropertyAttributes Attributes => _realProperty.Attributes;
        public override bool CanRead => _scriptMark.CanRead;
        public override bool CanWrite => _scriptMark.CanWrite;
        public override Type PropertyType => _realProperty.PropertyType;
        public object UnderlyingObject => _realProperty;
    }
}