using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class AttachedScriptsFactory
    {
        private Dictionary<string, LoadedModule> _loadedModules;
        private ScriptingEngine _engine;

        internal AttachedScriptsFactory(ScriptingEngine engine)
        {
            _loadedModules = new Dictionary<string, LoadedModule>(StringComparer.InvariantCultureIgnoreCase);
            _engine = engine;
        }

        public ModuleHandle AttachByPath(CompilerService compiler, string path, string typeName)
        {
            ThrowIfTypeExist(typeName);

            var code = _engine.Loader.FromFile(path);
            return LoadAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code);

        }

        public ModuleHandle AttachFromString(CompilerService compiler, string text, string typeName)
        {
            ThrowIfTypeExist(typeName);

            var code = _engine.Loader.FromString(text);
            return LoadAndRegister(typeof(AttachedScriptsFactory), compiler, typeName, code);
        }

        private void ThrowIfTypeExist(string typeName)
        {
            if (_loadedModules.ContainsKey(typeName))
            {
                throw new RuntimeException("Type «" + typeName + "» already registered");
            }

        }

        private ModuleHandle LoadAndRegister(Type type, CompilerService compiler, string typeName, Environment.ICodeSource code)
        {
            var moduleHandle = compiler.CreateModule(code);
            var loadedHandle = _engine.LoadModuleImage(moduleHandle);
            _loadedModules.Add(typeName, loadedHandle.Module);

            TypeManager.RegisterType(typeName, type);

            return moduleHandle;
        }

        private static AttachedScriptsFactory _instance;

        static AttachedScriptsFactory()
        {
        }

        internal static void SetInstance(AttachedScriptsFactory factory)
        {
            _instance = factory;
        }

        public static void Dispose()
        {
            _instance = null;
        }

        [ScriptConstructor(ParametrizeWithClassName = true)]
        public static IRuntimeContextInstance ScriptFactory(string typeName, IValue[] arguments)
        {
            var module = _instance._loadedModules[typeName];

            var newObj = new UserScriptContextInstance(module, typeName);
            newObj.Initialize(_instance._engine.Machine);

            return newObj;
        }

    }
}
