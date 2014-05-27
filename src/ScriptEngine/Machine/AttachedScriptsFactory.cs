using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    class AttachedScriptsFactory
    {
        private Dictionary<string, LoadedModule> _loadedModules;
        private MachineInstance _machine;

        private AttachedScriptsFactory()
        {
            _loadedModules = new Dictionary<string, LoadedModule>();
        }

        public void AttachByPath(string path, string typeName)
        {
            if (_loadedModules.ContainsKey(typeName))
            {
                throw new RuntimeException("Type «" + typeName + "» already registered");
            }

            var source = ScriptSourceFactory.FileBased(path);
            var module = source.CreateModule().Module;
            _loadedModules.Add(typeName, new LoadedModule(module));
            TypeManager.RegisterType(typeName, typeof(AttachedScriptsFactory));

        }

        private static AttachedScriptsFactory _instance;

        static AttachedScriptsFactory()
        {
        }

        public static void Init(MachineInstance machine)
        {
            _instance = new AttachedScriptsFactory();
            _instance._machine = machine;
        }

        public static void Dispose()
        {
            _instance = null;
        }

        public static void Attach(string path, string typeName)
        {
            _instance.AttachByPath(path, typeName);
        }

        [ScriptConstructor(ParametrizeWithClassName = true)]
        public static IRuntimeContextInstance ScriptFactory(string typeName, IValue[] arguments)
        {
            var module = _instance._loadedModules[typeName];
            var newObj = new UserScriptContextInstance(module, typeName);
            _instance._machine.StateConsistentOperation(() =>
            {
                _instance._machine.AttachContext(newObj, true);
                _instance._machine.SetModule(module);
                _instance._machine.ExecuteModuleBody(module.EntryMethodIndex);
            });

            return newObj;
        }

    }
}
