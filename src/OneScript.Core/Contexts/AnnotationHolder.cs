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
    public class AnnotationHolder : ICustomAttributeProvider
    {
        private readonly object[] _annotations;

        public AnnotationHolder(object[] annotations)
        {
            _annotations = annotations;
        }

        public object[] GetCustomAttributes(bool inherit)
        {
            return _annotations ?? new object[0];
        }

        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return GetCustomAttributes(true).Where(x => attributeType.IsAssignableFrom(attributeType)).ToArray();
        }

        public bool IsDefined(Type attributeType, bool inherit)
        {
            return _annotations != default &&
                   _annotations.Any(x => x.GetType() == attributeType);
        }
    }
}