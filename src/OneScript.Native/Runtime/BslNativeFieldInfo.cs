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
using OneScript.Contexts;

namespace OneScript.Native.Runtime
{
    public class BslNativeFieldInfo : BslFieldInfo
    {
        private Type _declaringType;
        private bool _isPublic;
        
        public BslNativeFieldInfo(string name)
        {
            Name = name;
        }

        public void SetDeclaringType(Type declType)
        {
            _declaringType = declType;
        }

        public void SetExportFlag(bool isExported)
        {
            _isPublic = isExported;
        }

        public void SetAnnotations(IEnumerable<object> attributes)
        {
            SetAnnotations(new AnnotationHolder(attributes.ToArray()));
        }
        
        public override Type DeclaringType => _declaringType;
        public override string Name { get; }
        public override Type ReflectedType => _declaringType;
        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override FieldAttributes Attributes { get; }
        public override RuntimeFieldHandle FieldHandle { get; }
        public override Type FieldType { get; }
    }
}