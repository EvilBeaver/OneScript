﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    static class ContextDiscoverer
    {
        private const string INSTANCE_RETRIEVER_NAME = "CreateInstance";
        
        public static void DiscoverClasses(System.Reflection.Assembly assembly)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes().AsParallel();
            }
            catch (ReflectionTypeLoadException exc)
            {
                var sb = new StringBuilder();
                int i = 0;
                foreach (var loaderException in exc.LoaderExceptions)
                {
                    sb.AppendFormat("Inner exception [{0}]", i++);
                    sb.AppendLine(loaderException.ToString());
                }
                throw new Exception("Error loading assemblies:\n" + sb.ToString());
            }

            var collection = GetMarkedTypes(types, typeof(ContextClassAttribute));

            foreach (var type in collection)
            {
                RegisterSystemType(type);
            }
        }

        public static void DiscoverGlobalContexts(RuntimeEnvironment environment, System.Reflection.Assembly assembly)
        {
            var allTypes = assembly.GetTypes();
            var enums = GetMarkedTypes(allTypes.AsParallel(), typeof(SystemEnumAttribute));
            foreach (var item in enums)
            {
                RegisterSystemEnum(item, environment);
            }

            var simpleEnums = GetMarkedTypes(allTypes.AsParallel(), typeof(EnumerationTypeAttribute));
            foreach (var item in simpleEnums)
            {
                RegisterSimpleEnum(item, environment);
            }

            var contexts = GetMarkedTypes(allTypes.AsParallel(), typeof(GlobalContextAttribute));
            foreach (var item in contexts)
            {
                RegisterGlobalContext(item, environment);
            }
        }

        private static IEnumerable<Type> GetMarkedTypes(IEnumerable<Type> allTypes, Type attribute)
        {
            return allTypes.Where(t => t.IsDefined(attribute, false));
        }

        private static void RegisterSystemType(Type stdClass)
        {
            var attribData = stdClass.GetCustomAttributes(typeof(ContextClassAttribute), false);
            System.Diagnostics.Trace.Assert(attribData.Length > 0, "Class is not marked as context");

            var attr = (ContextClassAttribute)attribData[0];
            var newType = TypeManager.RegisterType(attr.GetName(), stdClass);
            string alias = attr.GetAlias();
            if(!String.IsNullOrEmpty(alias))
                TypeManager.RegisterAliasFor(newType, alias);
        }

        private static void RegisterSystemEnum(Type enumType, RuntimeEnvironment environment)
        {
            var method = enumType.GetMethod(INSTANCE_RETRIEVER_NAME, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            System.Diagnostics.Trace.Assert(method != null, "System enum must have a static method " + INSTANCE_RETRIEVER_NAME);

            var instance = (IValue)method.Invoke(null, null);
            GlobalsManager.RegisterInstance(instance);
            var enumMetadata = (SystemEnumAttribute)enumType.GetCustomAttributes(typeof(SystemEnumAttribute), false)[0];
            environment.InjectGlobalProperty(instance, enumMetadata.GetName(), true);
            if(enumMetadata.GetAlias() != String.Empty)
                environment.InjectGlobalProperty(instance, enumMetadata.GetAlias(), true);
        }

        private static void RegisterSimpleEnum(Type enumType, RuntimeEnvironment environment)
        {
            var enumTypeAttribute = (EnumerationTypeAttribute)enumType.GetCustomAttributes (typeof (EnumerationTypeAttribute), false)[0];

            var type = TypeManager.RegisterType ("Перечисление" + enumTypeAttribute.Name, typeof (EnumerationContext));
            if (enumTypeAttribute.Alias != null)
                TypeManager.RegisterAliasFor (type, "Enum" + enumTypeAttribute.Alias);

            var enumValueType = TypeManager.RegisterType (enumTypeAttribute.Name, enumType);

            var instance = new EnumerationContext (type, enumValueType);

            var wrapperTypeUndefined = typeof (CLREnumValueWrapper<>);
            var wrapperType = wrapperTypeUndefined.MakeGenericType (new Type [] { enumType } );
            var constructor = wrapperType.GetConstructor (new Type [] { typeof(EnumerationContext), enumType, typeof(DataType) });

            foreach (var field in enumType.GetFields())
            {
                foreach (var contextFieldAttribute in field.GetCustomAttributes (typeof (EnumItemAttribute), false))
                {
                    var contextField = (EnumItemAttribute)contextFieldAttribute;
                    var osValue = (EnumerationValue)constructor.Invoke (new object [] { instance, field.GetValue (null), DataType.Enumeration } );

                    if (contextField.Alias == null)
                    {
                        if(StringComparer
                             .InvariantCultureIgnoreCase
                             .Compare(field.Name, contextField.Name) != 0)
                            instance.AddValue(contextField.Name, field.Name, osValue);
                        else
                            instance.AddValue(contextField.Name, osValue);
                    }
                    else
                        instance.AddValue(contextField.Name, contextField.Alias, osValue);
                }
            }

			if (enumTypeAttribute.CreateGlobalProperty)
			{
				GlobalsManager.RegisterInstance(enumType, instance);
				environment.InjectGlobalProperty(instance, enumTypeAttribute.Name, true);
				if (enumTypeAttribute.Alias != null)
					environment.InjectGlobalProperty(instance, enumTypeAttribute.Alias, true);
			}
        }


        private static void RegisterGlobalContext(Type contextType, RuntimeEnvironment environment)
        {
            var attribData = (GlobalContextAttribute)contextType.GetCustomAttributes(typeof(GlobalContextAttribute), false)[0];
            if (attribData.ManualRegistration)
                return;

            var method = contextType.GetMethod(INSTANCE_RETRIEVER_NAME, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            System.Diagnostics.Trace.Assert(method != null, "Global context must have a static method " + INSTANCE_RETRIEVER_NAME);
            var instance = (IAttachableContext)method.Invoke(null, null);
            GlobalsManager.RegisterInstance(instance);
            environment.InjectObject(instance, false);

        }

    }
}
