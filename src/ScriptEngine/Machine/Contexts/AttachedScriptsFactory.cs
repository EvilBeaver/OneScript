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

        public void AttachByPath(CompilerService compiler, string path, string typeName)
        {
            if (_loadedModules.ContainsKey(typeName))
            {
                throw new RuntimeException("Type «" + typeName + "» already registered");
            }

            var code = _engine.Loader.FromFile(path);
            var moduleHandle = compiler.CreateModule(code);
            var loadedHandle = _engine.LoadModuleImage(moduleHandle);
            _loadedModules.Add(typeName, loadedHandle.Module);
            
            TypeManager.RegisterType(typeName, typeof(AttachedScriptsFactory));

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
