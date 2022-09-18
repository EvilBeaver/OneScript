/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using OneScript.Compilation;
using OneScript.Values;

namespace OneScript.Contexts.Internal
{
    internal class ExternalPropertyInfo : BslPropertyInfo
    {
        private readonly int _valueIndex;
        
        private readonly MethodInfo _getter;
        private readonly MethodInfo _setter;
        
        public ExternalPropertyInfo(string name, string alias, GlobalPropertiesHolder holder, int valueIndex, bool writable = false)
        {
            _valueIndex = valueIndex;
            
            var target = holder.GetPropValue(valueIndex);
            var targetType = target.GetType();
            
            _getter = MakeGetter(targetType);
            _setter = MakeSetter(targetType);
            
            Alias = alias;
            Name = name;
            
            DeclaringType = targetType.DeclaringType;
            ReflectedType = targetType.DeclaringType;
            PropertyType = targetType;
            CanWrite = writable;
        }

        public int DispatchId => _valueIndex;
        
        private static MethodInfo MakeGetter(Type propType)
        {
            var rawMethod = typeof(ExternalPropertyInfo).GetMethod("GenericGetter", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(rawMethod != null);

            return rawMethod.MakeGenericMethod(propType);
        }
        
        private static MethodInfo MakeSetter(Type propType)
        {
            var rawMethod = typeof(ExternalPropertyInfo).GetMethod("GenericSetter", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Assert(rawMethod != null);

            return rawMethod.MakeGenericMethod(propType);
        }

        // ReSharper disable once UnusedMember.Local
        private T GenericGetter<T>(GlobalPropertiesHolder holder) where T : BslValue
        {
            return (T)holder.GetPropValue(_valueIndex);
        }
        
        // ReSharper disable once UnusedMember.Local
        private void GenericSetter<T>(GlobalPropertiesHolder holder, T value) where T : BslValue
        {
            holder.SetPropValue(_valueIndex, value);
        }

        public override Type DeclaringType { get; }
        
        public override string Alias { get; }
        public override bool Equals(BslPropertyInfo other)
        {
            return other is ExternalPropertyInfo ext 
                    && ext._valueIndex == _valueIndex
                    && ext.DeclaringType == DeclaringType;
        }

        public override string Name { get; }
        
        public override Type ReflectedType { get; }

        public override MethodInfo[] GetAccessors(bool nonPublic) => new MethodInfo[] { _getter, _setter };

        public override MethodInfo GetGetMethod(bool nonPublic) => _getter;

        public override ParameterInfo[] GetIndexParameters() => Array.Empty<ParameterInfo>();

        public override MethodInfo GetSetMethod(bool nonPublic) => _setter;

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (!(obj is GlobalPropertiesHolder holder))
            {
                throw new InvalidOperationException($"Target is not of type {nameof(GlobalPropertiesHolder)}");
            }

            return holder.GetPropValue(_valueIndex);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (!(obj is GlobalPropertiesHolder holder))
            {
                throw new InvalidOperationException($"Target is not of type {nameof(GlobalPropertiesHolder)}");
            }

            if (value != null && !PropertyType.IsInstanceOfType(value))
            {
                throw new ArgumentException($"Invalid value type {value.GetType()}");
            }

            holder.SetPropValue(_valueIndex, (BslValue)value);
        }

        public override PropertyAttributes Attributes => PropertyAttributes.None;

        public override bool CanRead => true;

        public override bool CanWrite { get; }

        public override Type PropertyType { get; }
    }
}