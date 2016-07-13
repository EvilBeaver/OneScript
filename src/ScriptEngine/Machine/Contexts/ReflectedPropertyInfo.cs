/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ScriptEngine.Machine.Contexts
{
    class ReflectedPropertyInfo : PropertyInfo
    {
        bool _canRead;
        bool _canWrite;

        readonly string _name;
        int _dispId;

        public ReflectedPropertyInfo(string name)
            : base()
        {
            _name = name;
            SetReadable(true);
            SetWritable(true);
        }

        internal void SetReadable(bool val)
        {
            _canRead = val;
        }
        internal void SetWritable(bool val)
        {
            _canWrite = val;
        }

        public override PropertyAttributes Attributes
        {
            get { return PropertyAttributes.None; }
        }

        public override bool CanRead
        {
            get { return _canRead; }
        }

        public override bool CanWrite
        {
            get { return _canWrite; }
        }

        public override System.Reflection.MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override System.Reflection.MethodInfo GetGetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override System.Reflection.MethodInfo GetSetMethod(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            IRuntimeContextInstance inst = obj as IRuntimeContextInstance;
            if (inst == null)
                throw new ArgumentException("Wrong argument type");

            IValue retVal = inst.GetPropValue(_dispId);

            return COMWrapperContext.MarshalIValue(retVal);
        }

        public override Type PropertyType
        {
            get { return typeof(IValue); }
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, System.Globalization.CultureInfo culture)
        {
            IRuntimeContextInstance inst = obj as IRuntimeContextInstance;
            if (inst == null)
                throw new ArgumentException("Wrong argument type");

            inst.SetPropValue(_dispId, COMWrapperContext.CreateIValue(value));
        }

        public override Type DeclaringType
        {
            get { return typeof(ReflectableSDO); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(DispIdAttribute))
            {
                return this.GetCustomAttributes(inherit);
            }
            else
            {
                return new object[0];
            }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            DispIdAttribute[] attribs = new DispIdAttribute[1];
            attribs[0] = new DispIdAttribute(_dispId);
            return attribs;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(DispIdAttribute);
        }

        public override string Name
        {
            get { return _name; }
        }

        public override Type ReflectedType
        {
            get { return typeof(ReflectableSDO); }
        }

        internal void SetDispId(int p)
        {
            _dispId = p;
        }
    }
}
//#endif