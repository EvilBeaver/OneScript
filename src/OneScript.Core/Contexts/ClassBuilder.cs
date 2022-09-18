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
using OneScript.Execution;

namespace OneScript.Contexts
{
    public class ClassBuilder
    {
        private readonly Type _classType;
        private readonly List<BslMethodInfo> _methods = new List<BslMethodInfo>();
        private readonly List<BslPropertyInfo> _properties = new List<BslPropertyInfo>();
        private readonly List<BslFieldInfo> _fields = new List<BslFieldInfo>();
        private readonly List<BslConstructorInfo> _constructors = new List<BslConstructorInfo>();

        public ClassBuilder(Type classType)
        {
            _classType = classType;
        }
        
        public string TypeName { get; set; }
        
        public IExecutableModule Module { get; set; }

        public ClassBuilder SetTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public ClassBuilder SetModule(IExecutableModule module)
        {
            Module = module;
            return this;
        }

        public ClassBuilder ExportClassMethod(string methodName)
        {
            var mi = _classType.GetMethod(methodName);
            if(mi == null)
                throw new InvalidOperationException($"Method '{methodName}' not found in {_classType}");

            ExportClassMethod(mi);
            return this;
        }

        public ClassBuilder ExportClassMethod(MethodInfo nativeMethod)
        {
            if(nativeMethod == null)
                throw new ArgumentNullException(nameof(nativeMethod));

            if (nativeMethod.ReflectedType != _classType)
                throw new InvalidOperationException($"Method '{nativeMethod.Name}' not found in {_classType}");

            if (MarkedAsContextMethod(nativeMethod, true))
            {
                _methods.Add(new ContextMethodInfo(nativeMethod));
            }
            else
            {
                var markup = new ContextMethodAttribute(nativeMethod.Name);
                _methods.Add(new ContextMethodInfo(nativeMethod, markup));
            }

            return this;
        }

        public ClassBuilder ExportProperty(string propName)
        {
            var info = _classType.GetProperty(propName);
            if (info == null)
                throw new InvalidOperationException($"Property '{propName}' not found in {_classType}");

            _properties.Add(new ContextPropertyInfo(info));
            
            return this;
        }

        public ClassBuilder ExportMethods(bool includeDeprecations = false)
        {
            var methods = _classType.GetMethods()
                                   .Where(x => MarkedAsContextMethod(x, includeDeprecations))
                                   .Select(x=>new ContextMethodInfo(x));

            _methods.AddRange(methods);
            return this;
        }
        
        public ClassBuilder ExportProperties(bool includeDeprecations = false)
        {
            var props = _classType.GetProperties()
                                   .Where(prop => MarkedAsContextProperty(prop, includeDeprecations))
                                   .Select(prop => new ContextPropertyInfo(prop));
            
            _properties.AddRange(props);
            return this;
        }

        private bool MarkedAsContextMethod(MemberInfo member, bool includeDeprecations)
        {
            return member
                  .GetCustomAttributes(typeof(ContextMethodAttribute), false)
                  .Any(x => includeDeprecations || (x as ContextMethodAttribute)?.IsDeprecated == false);
        }

        private bool MarkedAsContextProperty(MemberInfo member, bool includeDeprecations = false)
        {
            return member.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Any();
        }

        public ClassBuilder ExportConstructor(MethodInfo info)
        {
            if (info.DeclaringType != _classType)
            {
                throw new ArgumentException("info must belong to the current class");
            }

            _constructors.Add(new ContextConstructorInfo(info));
            return this;
        }

        public ClassBuilder ExportConstructor(InstanceConstructor creator)
        {
            IBuildableMember info = new BslLambdaConstructorInfo(creator);
            info.SetDeclaringType(_classType);
            _constructors.Add((BslLambdaConstructorInfo)info);
            return this;
        }

        public ClassBuilder ExportScriptVariables()
        {
            if(Module == null)
                throw new InvalidOperationException("Module is not set");

            foreach (var variable in Module.Fields)
            {
                _fields.Add(variable);
            }

            return this;
        }

        public ClassBuilder ExportScriptMethods()
        {
            if (Module == null)
                throw new InvalidOperationException("Module is not set");

            foreach (var methodDescriptor in Module.Methods)
            {
                if(methodDescriptor.Name == IExecutableModule.BODY_METHOD_NAME)
                    continue;
                
                _methods.Add(methodDescriptor);
            }

            return this;
        }

        public ClassBuilder ExportScriptConstructors()
        {
            var statics = _classType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                   .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute));

            foreach (var staticConstructor in statics)
            {
                ExportConstructor(staticConstructor);
            }

            return this;
        }

        public ClassBuilder ExportDefaults()
        {
            ExportMethods();
            ExportProperties();
            if (Module != null)
            {
                ExportScriptVariables();
                ExportScriptMethods();
                ExportScriptConstructors();
            }

            return this;
        }

        public Type Build()
        {
            var classDelegator = new ReflectedClassType(_classType);
            classDelegator.SetName(TypeName);
            classDelegator.SetFields(_fields);
            classDelegator.SetProperties(_properties);
            classDelegator.SetMethods(_methods);
            classDelegator.SetConstructors(_constructors);

            return classDelegator;
        }
        
    }
    
}
