/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptEngine.HostedScript
{
    public class LibraryLoader : ThisAwareScriptedObjectBase
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
        
        private LibraryLoader(LoadedModule moduleHandle, RuntimeEnvironment env, ScriptingEngine engine): base(moduleHandle)
        {
            _env = env;
            _engine = engine;
            _customized = true;

            _engine.InitializeSDO(this);

        }

        private LibraryLoader(RuntimeEnvironment env, ScriptingEngine engine)
        {
            _env = env;
            _engine = engine;
            _customized = false;
        }
        
        #region Static part

        private static readonly ContextMethodsMapper<LibraryLoader> _methods = new ContextMethodsMapper<LibraryLoader>();

        public static LibraryLoader Create(ScriptingEngine engine, RuntimeEnvironment env, string processingScript)
        {
            var code = engine.Loader.FromFile(processingScript);
            var compiler = engine.GetCompilerService();
            RegisterSymbols(compiler);
            
            for (int i = 0; i < _methods.Count; i++)
            {
                var mi = _methods.GetMethodInfo(i);
                compiler.DefineMethod(mi);
            }

            var module = compiler.Compile(code);
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
            _engine.AttachExternalAssembly(assembly, _env);

        }

        [ContextMethod("ДобавитьМакет", "AddTemplate")]
        public void AddTemplate(string file, string name, TemplateKind kind = TemplateKind.File)
        {
            var manager = GlobalsManager.GetGlobalContext<TemplateStorage>();
            manager.RegisterTemplate(file, name, kind);
        }

        protected override int GetOwnVariableCount()
        {
            return 1;
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
            _delayLoadedScripts.Clear();
            
            if(!_customized)
            {
                LibraryResolver.TraceLoadLibrary(String.Format("Использую штатный package loader"));
                success = DefaultProcessing(libraryPath);
            }
            else
            {
                LibraryResolver.TraceLoadLibrary(String.Format("Использую НЕ штатный package loader"));
                success = CustomizedProcessing(libraryPath);
            }

            if (success)
            {
                var library = new ExternalLibraryDef(Path.GetFileName(libraryPath));
                CompileDelayedModules(library);
            } else {
                LibraryResolver.TraceLoadLibrary(String.Format("!!! - Ошибка работы Package LOADER - библиотека не будет загружена "));
            }

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
            var files = Directory.EnumerateFiles(libraryPath, "*.os", SearchOption.AllDirectories)
                .Select(x => new { Name = Path.GetFileNameWithoutExtension(x), Path = x })
                .Where(x => Utils.IsValidIdentifier(x.Name));

            bool hasFiles = false;

            LibraryResolver.TraceLoadLibrary(String.Format("Модулей в библиотеке {0} - {1}", libraryPath, files.Count()));

            foreach (var file in files)
            {
                LibraryResolver.TraceLoadLibrary(String.Format("Загружаю модуль библиотеки {0}", file.Path));
                hasFiles = true;
                AddModule(file.Path, file.Name);
            }

            return hasFiles;
        }

        private void CompileDelayedModules(ExternalLibraryDef library)
        {
            foreach (var scriptFile in _delayLoadedScripts)
            {
                if (scriptFile.asClass)
                {
                    library.AddClass(scriptFile.identifier, scriptFile.path);
                }
                else
                {
                    var module = library.AddModule(scriptFile.identifier, scriptFile.path);
                }
            }

            library.Modules.ForEach(moduleFile =>
            {
                var image = CompileFile(moduleFile.FilePath);
                moduleFile.Image = image;
            });
            
            library.Classes.ForEach(classFile =>
            {
                var image = CompileFile(classFile.FilePath);
                _engine.AttachedScriptsFactory.LoadAndRegister(classFile.Symbol, image);
                classFile.Image = image;
            });

            _env.InitExternalLibrary(_engine, library);
        }

        private ModuleImage CompileFile(string path)
        {
            var compiler = _engine.GetCompilerService();
            
            var source = _engine.Loader.FromFile(path);
            var module = _engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, source, null);

            return module;
        }
    }
}
