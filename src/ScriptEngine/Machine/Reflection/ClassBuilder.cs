using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SysReflection = System.Reflection;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public class ClassBuilder<T> where T: ScriptDrivenObject
    {
        private List<SysReflection.MethodInfo> _methods = new List<SysReflection.MethodInfo>();
        private List<SysReflection.PropertyInfo> _properties = new List<SysReflection.PropertyInfo>();
        private List<SysReflection.FieldInfo> _fields = new List<SysReflection.FieldInfo>();

        public string TypeName { get; set; }
        public LoadedModule Module { get; set; }

        public ClassBuilder<T> SetTypeName(string typeName)
        {
            TypeName = typeName;
            return this;
        }

        public ClassBuilder<T> SetModule(LoadedModule module)
        {
            Module = module;
            return this;
        }

        public ClassBuilder<T> ExportClassMethod(string methodName)
        {
            var mi = typeof(T).GetMethod(methodName);
            if(mi == null)
                throw new InvalidOperationException($"Method '{methodName}' not found in {typeof(T)}");

            return this;
        }

        public ClassBuilder<T> ExportProperty(string propName)
        {
            var mi = typeof(T).GetProperty(propName);
            if (mi == null)
                throw new InvalidOperationException($"Method '{propName}' not found in {typeof(T)}");

            return this;
        }

        public ClassBuilder<T> ExportClasses(bool includeDeprecations = false)
        {
            var methods = typeof(T).GetMethods()
                                   .Where(x => MarkedAsContextMethod(x, includeDeprecations));
            _methods.AddRange(methods);
            return this;
        }

        public ClassBuilder<T> ExportProperties(bool includeDeprecations = false)
        {
            var methods = typeof(T).GetMethods()
                                   .Where(MarkedAsContextProperty);
            _methods.AddRange(methods);
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

        public ClassBuilder<T> ExportScriptFields()
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
                fieldInfo.SetDeclaringType(typeof(T));
                _fields.Add(fieldInfo);
            }

            return this;
        }

        public ClassBuilder<T> ExportScriptMethods()
        {
            if (Module == null)
                throw new InvalidOperationException("Module is not set");

            foreach (var methodDescriptor in Module.Methods)
            {
                var methInfo = CreateMethodInfo(methodDescriptor.Signature);
                _methods.Add(methInfo);
            }

            return this;
        }

        private ReflectedMethodInfo CreateMethodInfo(MethodInfo methInfo)
        {
            var reflectedMethod = new ReflectedMethodInfo(methInfo.Name);
            reflectedMethod.IsFunction = methInfo.IsFunction;
            for (int i = 0; i < methInfo.Params.Length; i++)
            {
                var currentParam = methInfo.Params[i];
                var reflectedParam = new ReflectedParamInfo(currentParam.Name, currentParam.IsByValue);
                reflectedParam.SetOwner(reflectedMethod);
                reflectedParam.SetPosition(i);
                reflectedMethod.Parameters.Add(reflectedParam);
            }

            return reflectedMethod;

        }
    }
}
