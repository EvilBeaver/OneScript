/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Values;

namespace ScriptEngine.Machine.Contexts
{
    public class SystemPropertyInfo : BslPropertyInfo, ISupportsDeprecation
    {
        private string _name;
        private string _alias;
        private Type _declaringType;
        private Type _propertyType;
        
        internal class Builder
        {
            private readonly SystemPropertyInfo _info;
            
            public Builder(string name, string alias = null)
            {
                _info = new SystemPropertyInfo();
                
                _info._name = name;
                _info._alias = alias;
                _info._declaringType = typeof(PropertyBag);
                _info._propertyType = typeof(BslValue);
            }
            
            public Builder SetDeclaringType(Type declaringType)
            {
                _info._declaringType = declaringType;
                return this;
            }
            
            public Builder SetPropertyType(Type propertyType)
            {
                _info._propertyType = propertyType;
                return this;
            }
            
            public Builder SetDeprecated(bool isDeprecated)
            {
                _info.IsDeprecated = isDeprecated;
                return this;
            }
            
            public SystemPropertyInfo Build()
            {
                return _info;
            }
        }

        private SystemPropertyInfo()
        {
            
        }
        
        
        // ReSharper disable once ConvertToAutoProperty
        public override Type DeclaringType => _declaringType;
        // ReSharper disable once ConvertToAutoProperty
        public override string Name => _name;
        // ReSharper disable once ConvertToAutoProperty
        public override string Alias => _alias;
        
        public bool IsDeprecated { get; private set; }
        
        public override bool Equals(BslPropertyInfo other)
        {
            if (! (other is SystemPropertyInfo gcProp))
                return false;
            
            return gcProp._name == _name &&
                   gcProp._alias == _alias &&
                   gcProp._declaringType == _declaringType &&
                   gcProp._propertyType == _propertyType;
        }

        public override Type ReflectedType => _declaringType;
        
        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            throw new NotImplementedException();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override PropertyAttributes Attributes { get; }
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override Type PropertyType => _propertyType;
    }
}