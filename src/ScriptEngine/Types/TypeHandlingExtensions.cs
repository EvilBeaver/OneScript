/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Types;

namespace ScriptEngine.Types
{
    public static class TypeHandlingExtensions
    {
        public static TypeDescriptor RegisterClass(this ITypeManager manager, Type classType)
        {
            var attribData = classType.GetCustomAttributes(typeof(ContextClassAttribute), false);
            if (attribData.Length == 0)
            {
                throw new InvalidOperationException("Class is not marked as context");
            }

            var attr = (ContextClassAttribute)attribData[0];

            var type = new TypeDescriptor(
                classType,
                attr.Name,
                attr.Alias);

            manager.RegisterType(type);
            return type;
        }

        public static TypeDescriptor GetTypeFromClassMarkup(this Type classType)
        {
            var attribData = classType.GetCustomAttributes(typeof(ContextClassAttribute), false);
            if (attribData.Length == 0)
            {
                throw new InvalidOperationException("Class is not marked as context");
            }

            var attr = (ContextClassAttribute)attribData[0];
            
            return new TypeDescriptor(
                classType,
                attr.Name,
                attr.Alias);
        }
    }
}