/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SysReflection = System.Reflection;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public class ClassBuilder : IReflectedClassBuilder
    {
        private readonly Type _classType;
        private List<SysReflection.MethodInfo> _methods = new List<SysReflection.MethodInfo>();
        private List<SysReflection.PropertyInfo> _properties = new List<SysReflection.PropertyInfo>();
        private List<SysReflection.FieldInfo> _fields = new List<SysReflection.FieldInfo>();
        private List<SysReflection.ConstructorInfo> _constructors = new List<SysReflection.ConstructorInfo>();

        public ClassBuilder(Type classType)
        {
            _classType = classType;
        }
        
        public string TypeName { get; set; }
        public LoadedModule Module { get; set; }

        public IReflectedClassBuilder SetTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public IReflectedClassBuilder SetModule(LoadedModule module)
        {
            Module = module;
            return this;
        }

        public IReflectedClassBuilder ExportClassMethod(string methodName)
        {
            var mi = _classType.GetMethod(methodName);
            if(mi == null)
                throw new InvalidOperationException($"Method '{methodName}' not found in {_classType}");

            ExportClassMethod(mi);
            return this;
        }

        public IReflectedClassBuilder ExportClassMethod(SysReflection.MethodInfo nativeMethod)
        {
            if(nativeMethod == null)
                throw new ArgumentNullException(nameof(nativeMethod));

            if (nativeMethod.ReflectedType != _classType)
                throw new InvalidOperationException($"Method '{nativeMethod.Name}' not found in {_classType}");

            if(MarkedAsContextMethod(nativeMethod, true))
                _methods.Add(new ContextMethodInfo(nativeMethod));
            else
                _methods.Add(nativeMethod);

            return this;
        }

        public IReflectedClassBuilder ExportProperty(string propName)
        {
            var info = _classType.GetProperty(propName);
            if (info == null)
                throw new InvalidOperationException($"Property '{propName}' not found in {_classType}");

            _properties.Add(new ContextPropertyInfo(info));
            
            return this;
        }

        public IReflectedClassBuilder ExportMethods(bool includeDeprecations = false)
        {
            var methods = _classType.GetMethods()
                                   .Where(x => MarkedAsContextMethod(x, includeDeprecations))
                                   .Select(x=>new ContextMethodInfo(x));

            _methods.AddRange(methods);
            return this;
        }
        
        public IReflectedClassBuilder ExportProperties(bool includeDeprecations = false)
        {
            var props = _classType.GetProperties()
                                   .Where(MarkedAsContextProperty);
            _properties.AddRange(props);
            return this;
        }

        private bool MarkedAsContextMethod(SysReflection.MemberInfo member, bool includeDeprecations)
        {
            return member
                  .GetCustomAttributes(typeof(ContextMethodAttribute), false)
                  .Any(x => includeDeprecations || (x as ContextMethodAttribute)?.IsDeprecated == false);
        }

        private bool MarkedAsContextProperty(SysReflection.MemberInfo member)
        {
            return member.GetCustomAttributes(typeof(ContextPropertyAttribute), false).Any();
        }

        public IReflectedClassBuilder ExportConstructor(SysReflection.ConstructorInfo info)
        {
            if (info.DeclaringType != _classType)
            {
                throw new ArgumentException("info must belong to the current class");
            }

            _constructors.Add(info);
            return this;
        }

        public IReflectedClassBuilder ExportConstructor(Func<object[], IRuntimeContextInstance> creator)
        {
            var info = new ReflectedConstructorInfo(creator);
            info.SetDeclaringType(_classType);
            _constructors.Add(info);
            return this;
        }

        public IReflectedClassBuilder ExportScriptVariables()
        {
            if(Module == null)
                throw new InvalidOperationException("Module is not set");

            foreach (var variable in Module.Variables)
            {
                var exported = Module.ExportedProperies.FirstOrDefault(x => x.SymbolicName.Equals(variable.Identifier) || x.SymbolicName.Equals(variable.Alias));
                bool exportFlag = exported.SymbolicName != null;
                if(exportFlag)
                    System.Diagnostics.Debug.Assert(variable.Index == exported.Index, "indices of vars and exports are equal");

                var fieldInfo = new ReflectedFieldInfo(variable, exportFlag);
                if (variable.Annotations != null)
                {
                    foreach (var annotation in variable.Annotations)
                    {
                        fieldInfo.AddAnnotation(annotation);
                    }
                }

                _fields.Add(fieldInfo);
            }

            return this;
        }

        public IReflectedClassBuilder ExportScriptMethods()
        {
            if (Module == null)
                throw new InvalidOperationException("Module is not set");

            for (int i = 0; i < Module.Methods.Length; i++)
            {
                var methodDescriptor = Module.Methods[i];
                if(methodDescriptor.Signature.Name == ModuleImage.BODY_METHOD_NAME)
                    continue;

                var methInfo = CreateMethodInfo(methodDescriptor.Signature);
                _methods.Add(methInfo);
            }

            return this;
        }

        private ReflectedMethodInfo CreateMethodInfo(MethodInfo methInfo)
        {
            var reflectedMethod = new ReflectedMethodInfo(methInfo.Name);
            reflectedMethod.SetPrivate(!methInfo.IsExport);
            reflectedMethod.IsFunction = methInfo.IsFunction;

            var unknownVal = ValueFactory.CreateInvalidValueMarker();

            for (int i = 0; i < methInfo.Params.Length; i++)
            {
                var currentParam = methInfo.Params[i];
                var reflectedParam = new ReflectedParamInfo(currentParam.Name, currentParam.IsByValue);
                reflectedParam.SetOwner(reflectedMethod);
                reflectedParam.SetPosition(i);
                if (currentParam.HasDefaultValue)
                {

                }

                reflectedParam.SetDefaultValue(unknownVal);
                if (currentParam.Annotations != null)
                {
                    foreach (var annotation in currentParam.Annotations)
                    {
                        reflectedParam.AddAnnotation(annotation);
                    }
                }

                reflectedMethod.Parameters.Add(reflectedParam);
            }

            if(methInfo.Annotations != null)
            {
                foreach (var annotation in methInfo.Annotations)
                {
                    reflectedMethod.AddAnnotation(annotation);
                }
            }

            return reflectedMethod;

        }

        public IReflectedClassBuilder ExportScriptConstructors()
        {
            var statics = _classType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                                   .Where(x => x.GetCustomAttributes(false).Any(y => y is ScriptConstructorAttribute));

            foreach (var staticConstructor in statics)
            {
                var action = new Func<object[], IRuntimeContextInstance>((parameters) =>
                {
                    return (IRuntimeContextInstance)staticConstructor.Invoke(null, SysReflection.BindingFlags.InvokeMethod, null, parameters, CultureInfo.CurrentCulture);
                });

                ExportConstructor(action);
            }

            return this;
        }

        public IReflectedClassBuilder ExportIndexer()
        {
            _properties.Add(new IndexerPropertyInfo(_classType));
            return this;
        }

        public IReflectedClassBuilder ExportDefaults()
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
