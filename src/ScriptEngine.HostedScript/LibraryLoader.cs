/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
        private readonly RuntimeEnvironment _env;
        private readonly ScriptingEngine _engine;

        readonly bool _customized;

        readonly List<DelayLoadedScriptData> _delayLoadedScripts = new List<DelayLoadedScriptData>();

        private struct DelayLoadedScriptData
        {
            public string path;
            public string identifier;
            public bool asClass;
        }

        private enum MethodNumbers
        {
            AddClass,
            AddProperty,
            LastNotAMethod
        }

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
        
        #region Static part

        private static readonly ContextMethodsMapper<LibraryLoader> _methods = new ContextMethodsMapper<LibraryLoader>();

        public static LibraryLoader Create(ScriptingEngine engine, RuntimeEnvironment env, string processingScript)
        {
            var code = engine.Loader.FromFile(processingScript);
            var compiler = engine.GetCompilerService();
            compiler.DefineVariable("ЭтотОбъект", SymbolType.ContextProperty);
            
            for (int i = 0; i < _methods.Count; i++)
            {
                var mi = _methods.GetMethodInfo(i);
                compiler.DefineMethod(mi);
            }

            var module = compiler.CreateModule(code);
            var loadedModule = engine.LoadModuleImage(module);

            return new LibraryLoader(loadedModule, env, engine);

        }

        public static LibraryLoader Create(ScriptingEngine engine, RuntimeEnvironment env)
        {
            return new LibraryLoader(env, engine);
        }

        #endregion

        [ContextMethod("ДобавитьКласс","AddClass")]
        public void AddClass(string file, string className)
        {
            if (!Utils.IsValidIdentifier(className))
                throw RuntimeException.InvalidArgumentValue();

            _delayLoadedScripts.Add(new DelayLoadedScriptData()
                {
                    path = file,
                    identifier = className,
                    asClass = true
                });
        }

        [ContextMethod("ДобавитьМодуль", "AddModule")]
        public void AddModule(string file, string moduleName)
        {
            if (!Utils.IsValidIdentifier(moduleName))
                throw RuntimeException.InvalidArgumentValue();

            _delayLoadedScripts.Add(new DelayLoadedScriptData()
            {
                path = file,
                identifier = moduleName,
                asClass = false
            });

            try
            {
                _env.InjectGlobalProperty(null, moduleName, true);
            }
            catch (InvalidOperationException e)
	        {
                // символ уже определен
                throw new RuntimeException(String.Format("Невозможно загрузить модуль {0}. Такой символ уже определен.", moduleName), e);
            }
        }

        [ContextMethod("ЗагрузитьБиблиотеку", "LoadLibrary")]
        public void LoadLibrary(string dllPath)
        {
            var assembly = System.Reflection.Assembly.LoadFrom(dllPath);
            _engine.AttachAssembly(assembly, _env);
        }

        protected override int GetOwnVariableCount()
        {
            return 1;
        }

        protected override int FindOwnProperty(string name)
        {
            if(StringComparer.OrdinalIgnoreCase.Compare(name, "ЭтотОбъект") == 0)
            {
                return 0;
            }

            return base.FindOwnProperty(name);
        }

        protected override bool IsOwnPropReadable(int index)
        {
            return true;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            if (index == 0)
                return this;
            else
                throw new ArgumentException(String.Format("Неверный индекс свойства {0}", index), "index");
        }

        protected override int GetOwnMethodCount()
        {
            return _methods.Count;
        }

        protected override void UpdateState()
        {
            
        }

        protected override int FindOwnMethod(string name)
        {
            return _methods.FindMethod(name);
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            return _methods.GetMethodInfo(index);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            _methods.GetMethod(index)(this, arguments);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return _methods.GetMethod(index)(this, arguments);
        }

        public bool ProcessLibrary(string libraryPath)
        {
            bool success;
            if(!_customized)
            {
                success = DefaultProcessing(libraryPath);
            }
            else
            {
                success = CustomizedProcessing(libraryPath);
            }

            if(success)
                CompileDelayedModules();

            return success;
        }

        private bool CustomizedProcessing(string libraryPath)
        {
            var libPathValue = ValueFactory.Create(libraryPath);
            var defaultLoading = Variable.Create(ValueFactory.Create(true), "$internalDefaultLoading");
            var cancelLoading = Variable.Create(ValueFactory.Create(false), "$internalCancelLoading");

            int eventIdx = GetScriptMethod("ПриЗагрузкеБиблиотеки", "OnLibraryLoad");
            if(eventIdx == -1)
            {
                return DefaultProcessing(libraryPath);
            }

            CallScriptMethod(eventIdx, new[] { libPathValue, defaultLoading, cancelLoading });

            if (cancelLoading.AsBoolean()) // Отказ = Ложь
                return false;

            if (defaultLoading.AsBoolean())
                return DefaultProcessing(libraryPath);

            return true;

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
                AddModule(file.Path, file.Name);
            }

            return hasFiles;
        }

        private void CompileDelayedModules()
        {
            var ordered = _delayLoadedScripts.OrderBy(x => x.asClass ? 1 : 0).ToArray();
            _delayLoadedScripts.Clear();

            foreach (var script in ordered)
            {
                var compiler = _engine.GetCompilerService();

                var source = _engine.Loader.FromFile(script.path);
                var module = _engine.AttachedScriptsFactory.CreateModuleFromSource(compiler, source, null);

                if(script.asClass)
                {
                    _engine.AttachedScriptsFactory.LoadAndRegister(script.identifier, module);
                    _env.NotifyClassAdded(module, script.identifier);
                }
                else
                {                    
                    _env.NotifyModuleAdded(module, script.identifier);
                }
            }

        }
    }
}
