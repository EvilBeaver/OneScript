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
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public class ReflectedFieldInfo : FieldInfo
    {
        private Type _declaringType;
        private bool _isPublic;
        private readonly VariableInfo _info;

        private readonly List<UserAnnotationAttribute> _attributes = new List<UserAnnotationAttribute>();

        public ReflectedFieldInfo(VariableInfo info, bool isPublic)
        {
            _info = info;
            _isPublic = isPublic;
        }
        
        public void SetDeclaringType(Type declType)
        {
            _declaringType = declType;
        }

        private IEnumerable<UserAnnotationAttribute> GetCustomAttributesInternal(bool inherit)
        {
            return _attributes;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return GetCustomAttributesInternal(inherit).ToArray();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            var attribs = GetCustomAttributesInternal(inherit);
            return attribs.Where(x => x.GetType() == attributeType).ToArray();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Any(x => x.GetType() == attributeType);
        }

        public override object GetValue(object obj)
        {
            var irc = obj as IRuntimeContextInstance;
            if(irc == null)
                throw new ArgumentException();

            return ContextValuesMarshaller.ConvertReturnValue(irc.GetPropValue(_info.Index));
        }
        
        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            var irc = obj as IRuntimeContextInstance;
            if (irc == null)
                throw new ArgumentException();

            irc.SetPropValue(_info.Index, (IValue) value);
        }

        public override string Name
        {
            get { return _info.Identifier; }
        }

        public override Type DeclaringType
        {
            get { return _declaringType; }
        }

        public override Type ReflectedType
        {
            get { return _declaringType; }
        }

        public override Type FieldType
        {
            get { return typeof(IValue); }
        }

        public override FieldAttributes Attributes
        {
            get { return _isPublic ? FieldAttributes.Public : FieldAttributes.Private; }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get { throw new NotImplementedException(); }
        }

        public void AddAnnotation(AnnotationDefinition annotation)
        {
            _attributes.Add(new UserAnnotationAttribute(annotation));
        }
    }
}
