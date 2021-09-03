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
using OneScript.Contexts.Internal;
using OneScript.Values;

namespace OneScript.Contexts
{
    public class BslParameterInfo : ParameterInfo, IBuildableMember
    {
        private AnnotationHolder _annotations;

        internal BslParameterInfo()
        {
            AttrsImpl = ParameterAttributes.In;
            ClassImpl = typeof(BslValue);
        }
        
        #region Attributes Infrastructure

        private AnnotationHolder Annotations => _annotations ??= new AnnotationHolder(new object[0]);

        public override object[] GetCustomAttributes(bool inherit)
        {
            return Annotations.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return Annotations.GetCustomAttributes(attributeType, inherit);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return Annotations.IsDefined(attributeType, inherit);
        }

        #endregion
        
        public override object DefaultValue => DefaultValueImpl;

        public override bool HasDefaultValue => DefaultValue != null || ConstantValueIndex != -1;
        
        public virtual bool ExplicitByVal { get; protected set; }

        public int ConstantValueIndex { get; set; } = -1;
        
        void IBuildableMember.SetDeclaringType(Type type)
        {
            MemberImpl = type;
        }

        void IBuildableMember.SetName(string name)
        {
            NameImpl = name;
        }

        void IBuildableMember.SetAlias(string alias)
        {
            throw new NotSupportedException();
        }

        void IBuildableMember.SetExportFlag(bool isExport)
        {
            throw new NotSupportedException();
        }

        void IBuildableMember.SetDataType(Type type)
        {
            ClassImpl = type;
        }

        void IBuildableMember.SetAnnotations(IEnumerable<object> annotations)
        {
            _annotations = new AnnotationHolder(annotations.ToArray());
        }

        void IBuildableMember.SetDispatchIndex(int index)
        {
            PositionImpl = index;
        }

        internal void ByValue(bool byVal)
        {
            ExplicitByVal = byVal;
        }
        
        internal void SetDefaultValue(BslPrimitiveValue val)
        {
            DefaultValueImpl = val;
            AttrsImpl |= ParameterAttributes.HasDefault | ParameterAttributes.Optional;
        }

        internal void SetOwner(MemberInfo parent)
        {
            MemberImpl = parent;
        }
    }
}