/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Reflection;

namespace OneScript.Contexts
{
    /// <summary>
    /// Информация о методе, который может быть вызван из 1Script
    /// </summary>
    public abstract class BslMethodInfo : MethodInfo, INameAndAliasProvider
    {
        private AnnotationHolder _annotations;
        
        public abstract string Alias { get; }
        
        private AnnotationHolder Annotations
        {
            get
            {
                if (_annotations == default)
                    _annotations = RetrieveAnnotations();

                return _annotations;
            }
        }

        protected void SetAnnotations(AnnotationHolder annotations)
        {
            _annotations = annotations;
        }

        protected virtual AnnotationHolder RetrieveAnnotations()
        {
            return new AnnotationHolder(new object[0]);
        }
        
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

        public override string ToString()
        {
            return $"{ReturnType} {Name}(${string.Join(',', GetParameters().Select(x=>x.ParameterType))})";
        }
    }
}