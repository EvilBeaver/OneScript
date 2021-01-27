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
using System.Text;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    public class ContextDiscoverer
    {
        private const string INSTANCE_RETRIEVER_NAME = "CreateInstance";

        public ContextDiscoverer(ITypeManager types, IGlobalsManager globals)
        {
            Types = types;
            Globals = globals;
        }
        
        private ITypeManager Types { get; }
        private IGlobalsManager Globals { get; }
        
        public void DiscoverClasses(Assembly assembly, Predicate<Type> filter = null)
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

            if (filter == null)
            {
                filter = t => true;
            }
            
            var collection = GetMarkedTypes(types, typeof(ContextClassAttribute), filter);

            foreach (var type in collection)
            {
                RegisterSystemType(type);
            }
        }

        public void DiscoverGlobalContexts(
            RuntimeEnvironment environment, 
            Assembly assembly,
            Predicate<Type> filter = null)
        {
            if (filter == null)
            {
                filter = t => true;
            }
            
            var allTypes = assembly.GetTypes();
            var enums = GetMarkedTypes(allTypes.AsParallel(), typeof(SystemEnumAttribute), filter);
            foreach (var item in enums)
            {
                RegisterSystemEnum(item, environment);
            }

            var simpleEnums = GetMarkedTypes(allTypes.AsParallel(), typeof(EnumerationTypeAttribute), filter);
            foreach (var item in simpleEnums)
            {
                RegisterSimpleEnum(item, environment);
            }

            var contexts = GetMarkedTypes(allTypes.AsParallel(), typeof(GlobalContextAttribute), filter);
            foreach (var item in contexts)
            {
                RegisterGlobalContext(item, environment);
            }
        }

        private static IEnumerable<Type> GetMarkedTypes(IEnumerable<Type> allTypes, Type attribute, Predicate<Type> filter)
        {
            return allTypes
                .Where(t => t.IsDefined(attribute, false) && filter(t));
        }

        private void RegisterSystemType(Type stdClass)
        {
            var attribData = stdClass.GetCustomAttributes(typeof(ContextClassAttribute), false);
            System.Diagnostics.Trace.Assert(attribData.Length > 0, "Class is not marked as context");

            var attr = (ContextClassAttribute)attribData[0];

            Types.RegisterType(attr.GetName(), attr.GetAlias(), stdClass);
        }

        private void RegisterSystemEnum(Type enumType, RuntimeEnvironment environment)
        {
            var method = enumType.GetMethod(INSTANCE_RETRIEVER_NAME, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            if(method == default)
                throw new ArgumentException($"System enum must have a static method {INSTANCE_RETRIEVER_NAME}");
            
            var parameters = method.GetParameters();
            if(parameters.Length != 1 || parameters[0].ParameterType != typeof(ITypeManager))
                throw new ArgumentException($"Method {enumType}::{INSTANCE_RETRIEVER_NAME} must accept ITypeManager as an argument");

            var enumMetadata = (SystemEnumAttribute)enumType.GetCustomAttributes(typeof(SystemEnumAttribute), false)[0];
            var instance = (IValue)method.Invoke(null, new object[]{Types});
            
            Globals.RegisterInstance(instance);
            
            environment.InjectGlobalProperty(instance, enumMetadata.GetName(), true);
            if(enumMetadata.GetAlias() != String.Empty)
                environment.InjectGlobalProperty(instance, enumMetadata.GetAlias(), true);
        }
        
        private void RegisterSimpleEnum(Type enumType, RuntimeEnvironment environment)
        {
            var enumTypeAttribute = (EnumerationTypeAttribute)enumType.GetCustomAttributes (typeof (EnumerationTypeAttribute), false)[0];

            var type = Types.RegisterType (
                "Перечисление" + enumTypeAttribute.Name,
                "Enum" + enumTypeAttribute.Alias,
                typeof (EnumerationContext));
            
            var enumValueType = Types.RegisterType (enumTypeAttribute.Name, enumTypeAttribute.Alias, enumType);

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
				Globals.RegisterInstance(enumType, instance);
				environment.InjectGlobalProperty(instance, enumTypeAttribute.Name, true);
				if (enumTypeAttribute.Alias != null)
					environment.InjectGlobalProperty(instance, enumTypeAttribute.Alias, true);
			}
        }


        private void RegisterGlobalContext(Type contextType, RuntimeEnvironment environment)
        {
            var attribData = (GlobalContextAttribute)contextType.GetCustomAttributes(typeof(GlobalContextAttribute), false)[0];
            if (attribData.ManualRegistration)
                return;

            var method = contextType.GetMethod(INSTANCE_RETRIEVER_NAME, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            System.Diagnostics.Trace.Assert(method != null, "Global context must have a static method " + INSTANCE_RETRIEVER_NAME);
            var instance = (IAttachableContext)method.Invoke(null, null);
            Globals.RegisterInstance(instance);
            environment.InjectObject(instance, false);

        }
    }
}
