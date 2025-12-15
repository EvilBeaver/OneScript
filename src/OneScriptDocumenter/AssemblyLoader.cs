/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OneScriptDocumenter
{
    class AssemblyLoader
    {
        readonly Type _classAttributeType;
        readonly Type _methodAttributeType;
        readonly Type _propAttributeType;
        readonly Type _constructorAttributeType;
        readonly Type _globalContextAttributeType;
        readonly Type _systemEnumAttribute;
        readonly Type _enumerationTypeAttribute;
        readonly Type _systemValue;
        readonly Type _enumValue;

        readonly string _baseDirectory;

        public AssemblyLoader(string baseDirectory)
        {
            _baseDirectory = baseDirectory;

            var engineFile = Path.Combine(_baseDirectory, "ScriptEngine.dll");

            if (!File.Exists(engineFile))
                throw new ArgumentException("Base directory doesn't contain library ScriptEngine.dll");

            var scriptEngineLib = Assembly.ReflectionOnlyLoadFrom(engineFile);

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (object sender, ResolveEventArgs args) =>
            {
                var data = args.Name.Split(',');
                var filename = Path.Combine(_baseDirectory, data[0] + ".dll");
                var asmLoaded = Assembly.ReflectionOnlyLoadFrom(filename);

                if (asmLoaded == null)
                    asmLoaded = Assembly.ReflectionOnlyLoad(args.Name);

                return asmLoaded;
            };

            _classAttributeType = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.ContextClassAttribute", true);
            _globalContextAttributeType = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.GlobalContextAttribute", true);
            _methodAttributeType = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.ContextMethodAttribute", true);
            _propAttributeType = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.ContextPropertyAttribute", true);
            _constructorAttributeType = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.ScriptConstructorAttribute", true);
            _systemEnumAttribute = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.SystemEnumAttribute", true);
            _systemValue = scriptEngineLib.GetType("ScriptEngine.Machine.Contexts.EnumValueAttribute", true);
            _enumerationTypeAttribute = scriptEngineLib.GetType("ScriptEngine.EnumerationTypeAttribute", true);
            _enumValue = scriptEngineLib.GetType("ScriptEngine.EnumItemAttribute", true);

            foreach (var name in new string[] { "ScriptEngine.HostedScript", "DotNetZip", "Newtonsoft.Json" })
            {
                var libFile = Path.Combine(_baseDirectory, name + ".dll");
                if (File.Exists(libFile))
                {
                    Assembly.ReflectionOnlyLoadFrom(libFile);
                }
            }

        }
        
        public LoadedAssembly Load(string assemblyName)
        {
            var library = Assembly.ReflectionOnlyLoadFrom(Path.Combine(_baseDirectory, assemblyName));

            var scriptEngineLibs = library.GetReferencedAssemblies()
                .Where(x => x.Name != "ScriptEngine");

            foreach (var lib in scriptEngineLibs)
            {
                try
                {
                    Assembly.ReflectionOnlyLoad(lib.FullName);
                }
                catch (FileNotFoundException)
                {
                    Assembly.ReflectionOnlyLoadFrom(Path.Combine(_baseDirectory, lib.Name + ".dll"));
                }
            }

            return new LoadedAssembly(library, this);
        }

        public Type MemberTypeToAttributeType(ScriptMemberType memberType)
        {
            switch (memberType)
            {
                case ScriptMemberType.Class:
                    return _classAttributeType;
                case ScriptMemberType.Constructor:
                    return _constructorAttributeType;
                case ScriptMemberType.GlobalContext:
                    return _globalContextAttributeType;
                case ScriptMemberType.Method:
                    return _methodAttributeType;
                case ScriptMemberType.Property:
                    return _propAttributeType;
                case ScriptMemberType.SystemEnum:
                    return _systemEnumAttribute;
                case ScriptMemberType.EnumerationType:
                    return _enumerationTypeAttribute;
                case ScriptMemberType.EnumerationValue:
                    return _systemValue;
                case ScriptMemberType.EnumItem:
                    return _enumValue;
                default:
                    throw new ArgumentException("Unsupported member type");
            }
        }

    }
}
