/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Contexts.Enums;
using OneScript.Localization;

namespace OneScriptDocumenter.Secondary
{
    public static class MarkupProvider
    {
        public static ContextClassAttribute GetClassMarkup(Type target)
        {
            return target.GetCustomAttribute<ContextClassAttribute>() ??
                   throw new ArgumentException($"Type {target} is not marked with class attribute");
        }
        
        public static INameAndAliasProvider GetEnumMarkup(Type target)
        {
            try
            {
                return target.GetCustomAttributes()
                    .Where(a => a is SystemEnumAttribute || a is EnumerationTypeAttribute)
                    .Cast<INameAndAliasProvider>()
                    .Single();
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException($"Type {target} is not marked with enum attribute", e);
            }
        }
        
        public static INameAndAliasProvider GetEnumValueMarkup(FieldInfo target)
        {
            try
            {
                return target.GetCustomAttribute<EnumValueAttribute>();
            }
            catch (AmbiguousMatchException e)
            {
                throw new ArgumentException($"Enum value {target} is not marked with enum attribute", e);
            }
        }
        
        public static ContextMethodAttribute GetMethodMarkup(MethodInfo target)
        {
            var markup = target.GetCustomAttribute<ContextMethodAttribute>()
                         ?? throw new ArgumentException($"Method {target} is not marked with method attribute");

            if (markup.SkipForDocumenter)
            {
                throw new ArgumentException($"Method {target} is skipped for documentation");
            }

            if (markup.IsDeprecated)
            {
                throw new ArgumentException($"Method {target} is deprecated");
            }

            return markup;
        }
        
        public static INameAndAliasProvider GetMemberDocumentationMarkup(MemberInfo target)
        {
            try
            {
                return target.GetCustomAttribute<DocumentedMemberAttribute>();
            }
            catch (AmbiguousMatchException e)
            {
                throw new ArgumentException($"Member {target} is not marked with docs attribute", e);
            }
        }
        
        public static ScriptConstructorAttribute GetConstructorMarkup(MethodInfo target)
        {
            try
            {
                return target.GetCustomAttribute<ScriptConstructorAttribute>();
            }
            catch (AmbiguousMatchException e)
            {
                throw new ArgumentException($"Method {target} is marked by several constructor attributes", e);
            }
        }
        
        public static ContextPropertyAttribute GetPropertyMarkup(PropertyInfo target)
        {
            try
            {
                return target
                    .GetCustomAttributes<ContextPropertyAttribute>()
                    .Single(attr => !attr.SkipForDocumenter);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException($"Property {target} is not marked with property attribute", e);
            }
        }
    }
}