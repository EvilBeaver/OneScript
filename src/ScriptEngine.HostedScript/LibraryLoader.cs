using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    class LibraryLoader : ScriptDrivenObject
    {
        private RuntimeEnvironment _env;
        private ScriptingEngine _engine;
        bool _customized;

        private Action<IValue[]>[] _callableMethods = new Action<IValue[]>[0]; // TODO: Сделать инициализацию

        private LibraryLoader(LoadedModuleHandle moduleHandle, RuntimeEnvironment _env, ScriptingEngine _engine): base(moduleHandle)
        {
            this._env = _env;
            this._engine = _engine;
            this._customized = true;
            _engine.InitializeSDO(this);

        }

        private LibraryLoader(RuntimeEnvironment _env, ScriptingEngine _engine)
            : base(new LoadedModuleHandle(), true)
        {
            this._env = _env;
            this._engine = _engine;
            this._customized = false;
        }

        public static LibraryLoader Create(ScriptingEngine engine, RuntimeEnvironment env, string processingScript)
        {
            var code = engine.Loader.FromFile(processingScript);
            var compiler = engine.GetCompilerService();
            var module = compiler.CreateModule(code);
            var loadedModule = engine.LoadModuleImage(module);

            return new LibraryLoader(loadedModule, env, engine);

        }

        public static LibraryLoader Create(ScriptingEngine engine, RuntimeEnvironment env)
        {
            return new LibraryLoader(env, engine);
        }


        protected override int GetVariableCount()
        {
            return 0;
        }

        protected override int GetMethodCount()
        {
            return _callableMethods.Length;
        }

        protected override void UpdateState()
        {
            
        }

        protected override int FindOwnMethod(string name)
        {
            throw new NotImplementedException();
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            throw new NotImplementedException();
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            throw new NotImplementedException();
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            throw new NotImplementedException();
        }

        public bool ProcessLibrary(string libraryPath)
        {
            if(!_customized)
            {
                return DefaultProcessing(libraryPath);
            }

            throw new NotImplementedException();
        }

        private bool DefaultProcessing(string libraryPath)
        {
            var files = Directory.EnumerateFiles(libraryPath, "*.os")
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Path = x })
                .Where(x => Utils.IsValidIdentifier(x.Name));

            bool hasFiles = false;

            foreach (var file in files)
            {
                hasFiles = true;
                var compiler = _engine.GetCompilerService();
                var instance = (IValue)_engine.AttachedScriptsFactory.LoadFromPath(compiler, file.Path);
                _env.InjectGlobalProperty(instance, file.Name, true);
            }

            return hasFiles;
        }
    }
}
