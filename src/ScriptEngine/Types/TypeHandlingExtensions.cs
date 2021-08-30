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

            if (attr.TypeUUID != default)
            {
                var type = new TypeDescriptor(
                    new Guid(attr.TypeUUID),
                    attr.GetName(),
                    attr.GetAlias(),
                    classType);

                manager.RegisterType(type);
                return type;
            }
            else
                return manager.RegisterType(attr.GetName(), attr.GetAlias(), classType);
        }

        public static TypeDescriptor GetTypeFromClassMarkup(this Type classType)
        {
            var attribData = classType.GetCustomAttributes(typeof(ContextClassAttribute), false);
            if (attribData.Length == 0)
            {
                throw new InvalidOperationException("Class is not marked as context");
            }

            var attr = (ContextClassAttribute)attribData[0];
            if (attr.TypeUUID == default)
            {
                throw new InvalidOperationException($"TypeUUID is not defined for {classType}");
            }
            
            return new TypeDescriptor(
                new Guid(attr.TypeUUID),
                attr.GetName(),
                attr.GetAlias(),
                classType);
        }
    }
}