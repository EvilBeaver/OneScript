/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OneScript.Values;

namespace OneScript.Contexts
{
    public class BslScriptPropertyInfo : BslPropertyInfo, IBuildableProperty
    {
        private Type _declaringType;
        private Type _propertyType = typeof(BslValue);
        private string _name;
        private string _alias;
        private bool _isPrivate = true;
        private bool _canRead = true;
        private bool _canWrite = true;
        
        protected BslScriptPropertyInfo()
        {
        }

        internal static BslScriptPropertyInfo Create()
        {
            return new BslScriptPropertyInfo();
        }

        public override Type DeclaringType => _declaringType;
        public override string Name => _name;
        public override string Alias => _alias;
        public override Type ReflectedType => _declaringType;
        public int DispatchId { get; private set; } = -1;
        
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

        public override PropertyAttributes Attributes => PropertyAttributes.None;
        public override bool CanRead => _canRead;
        public override bool CanWrite => _canWrite;
        public override Type PropertyType => _propertyType;
        
        void IBuildableMember.SetDeclaringType(Type type)
        {
            _declaringType = type;
        }

        void IBuildableMember.SetName(string name)
        {
            _name = name;
        }

        void IBuildableMember.SetAlias(string alias)
        {
            _alias = alias;
        }

        void IBuildableMember.SetExportFlag(bool isExport)
        {
            _isPrivate = !isExport;
        }

        void IBuildableMember.SetDataType(Type type)
        {
            _propertyType = type;
        }

        void IBuildableMember.SetAnnotations(IEnumerable<object> annotations)
        {
            SetAnnotations(new AnnotationHolder(annotations.ToArray()));
        }

        void IBuildableMember.SetDispatchIndex(int index)
        {
            DispatchId = index;
        }
        
        void IBuildableProperty.CanRead(bool canRead)
        {
            _canRead = canRead;
        }
        
        void IBuildableProperty.CanWrite(bool canWrite)
        {
            _canWrite = canWrite;
        }
    }
}