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
    public class IndexerPropertyInfo : PropertyInfo
    {
        private readonly Type _declaringType;

        public IndexerPropertyInfo(Type declaringType)
        {
            _declaringType = declaringType;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit) => false;

        public override Type ReflectedType => _declaringType;
        public override Type DeclaringType => _declaringType;
        public override string Name => "Item";
        
        public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new[]
            {
                GetGetMethod(),
                GetSetMethod()
            };
        }

        public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic)
        {
            return _declaringType.GetMethod(nameof(IRuntimeContextInstance.GetIndexedValue));
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            var isCollection = _declaringType.GetInterfaces().Contains(typeof(ICollectionContext));
            var isPropertyNamer = typeof(PropertyNameIndexAccessor).IsAssignableFrom(_declaringType);

            if (isCollection)
            {
                return new ParameterInfo[]
                {
                    new ReflectedParamInfo("index", true)
                };
            }

            if (isPropertyNamer)
            {
                var propNameParam = new TypeParameterInfo("index", typeof(string));
                propNameParam.SetOwner(this);
                propNameParam.SetPosition(0);

                return new ParameterInfo[]
                {
                    propNameParam
                };
            }

            return new ParameterInfo[0];
        }

        public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override PropertyAttributes Attributes { get; }
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override Type PropertyType => typeof(IValue);
    }
}