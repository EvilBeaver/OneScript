/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ScriptEngine.Machine.Reflection;

namespace ScriptEngine.Machine.Contexts
{
    public class ReflectedParamInfo : ParameterInfo
    {
        private readonly List<object> _attributes;

        public ReflectedParamInfo(string name, bool isByVal)
        {
            NameImpl = name;
            AttrsImpl = ParameterAttributes.In;
            if (!isByVal)
            {
                _attributes.Add(new ByRefAttribute());
            }

            ClassImpl = typeof(IValue);
            _attributes = new List<object>();
        }

        internal void SetOwner(MemberInfo owner)
        {
            MemberImpl = owner;
        }

        internal void SetPosition(int index)
        {
            PositionImpl = index;
        }

        public void AddAnnotation(AnnotationDefinition annotation)
        {
            _attributes.Add(new UserAnnotationAttribute()
            {
                Annotation = annotation
            });
        }

        public void SetDefaultValue(IValue val)
        {
            DefaultValueImpl = val;
        }

        public override object DefaultValue => DefaultValueImpl;

        public override bool HasDefaultValue => DefaultValue != null;

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _attributes.ToArray();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Where(x => x.GetType() == attributeType).ToArray();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Any(x => x.GetType() == attributeType);
        }
    }
}
//#endif