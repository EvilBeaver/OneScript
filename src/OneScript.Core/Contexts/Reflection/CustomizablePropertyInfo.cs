/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Reflection;

namespace OneScript.Contexts.Reflection
{
    public class CustomizablePropertyInfo : BslPropertyInfoBase
    {
        public override Type DeclaringType { get; }
        public override string Name { get; }
        public override Type ReflectedType { get; }
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

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override PropertyAttributes Attributes { get; }
        public override bool CanRead { get; }
        public override bool CanWrite { get; }
        public override Type PropertyType { get; }

        public void SetName(string propName)
        {
            throw new NotImplementedException();
        }

        public void SetAccessibility(bool isReadable, bool isWritable)
        {
            throw new NotImplementedException();
        }

        public void SetDispatchId(int propertyNumber)
        {
            throw new NotImplementedException();
        }
    }
}