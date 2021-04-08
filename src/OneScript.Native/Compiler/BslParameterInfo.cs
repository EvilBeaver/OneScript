/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    public class BslParameterInfo : ParameterInfo
    {
        private readonly List<Attribute> _attributes;

        public BslParameterInfo(string name) : this(name, typeof(BslValue))
        {
        }
        
        public BslParameterInfo(string name, Type type)
        {
            _attributes = new List<Attribute>();
            NameImpl = name;
            AttrsImpl = ParameterAttributes.In;
            ClassImpl = type;
        }

        internal void SetOwner(MemberInfo owner)
        {
            MemberImpl = owner;
        }

        internal void SetPosition(int index)
        {
            PositionImpl = index;
        }

        public void AddAttribute(Attribute value)
        {
            _attributes.Add(value);
        }
        
        public void SetDefaultValue(BslPrimitiveValue val)
        {
            DefaultValueImpl = val;
            AttrsImpl |= ParameterAttributes.HasDefault | ParameterAttributes.Optional;
        }

        public void SetByVal()
        {
            ExplicitByVal = true;
        }
        
        public void SetByRef()
        {
            ExplicitByVal = false;
            ClassImpl = ClassImpl.MakeByRefType();
        }

        public override object DefaultValue => DefaultValueImpl;

        public override bool HasDefaultValue => DefaultValue != null;
        
        public bool ExplicitByVal { get; private set; }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return _attributes.ToArray();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                return GetCustomAttributes(inherit);
            
            return _attributes.Where(x => attributeType.IsAssignableFrom(x.GetType()))
                .ToArray();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(inherit).Any(x => x.GetType() == attributeType);
        }
    }
}