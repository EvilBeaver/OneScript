/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__

using System;
using System.Collections.Generic;
using System.Reflection;

using ScriptEngine.Machine.Reflection;

namespace ScriptEngine.Machine.Contexts
{
    public class ReflectedParamInfo : ParameterInfo
    {
        private List<Attribute> _annotations;

        public ReflectedParamInfo(string name, bool isByVal)
        {
            NameImpl = name;
            AttrsImpl = ParameterAttributes.In;
            if (!isByVal)
            {
                AttrsImpl |= ParameterAttributes.Out;
            }

            ClassImpl = typeof(IValue);
            _annotations= new List<Attribute>();
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
            _annotations.Add(new UserAnnotationAttribute()
            {
                Annotation = annotation
            });
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _annotations.ToArray();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(UserAnnotationAttribute))
                return GetCustomAttributes(inherit);
            else
                return base.GetCustomAttributes(attributeType, inherit);
        }
    }
}
//#endif