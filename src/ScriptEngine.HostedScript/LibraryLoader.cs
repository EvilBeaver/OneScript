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
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Sources;

namespace ScriptEngine.HostedScript
{
    public class LibraryLoader : AutoScriptDrivenObject<LibraryLoader>
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
        
        private LibraryLoader(IExecutableModule moduleHandle, RuntimeEnvironment env, ScriptingEngine engine): base(moduleHandle)
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
        
        public static LibraryLoader Create(ScriptingEngine engine, string processingScript)
        {
            var compiler = engine.GetCompilerService();
            var code = engine.Loader.FromFile(processingScript);
            var module = CompileModule(compiler, code);
            
            return new LibraryLoader(module, engine.Environment, engine);

        }

        public static LibraryLoader Create(ScriptingEngine engine)
        {
            return new LibraryLoader(engine.Environment, engine);
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
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Загружаю модуль ={moduleName}= в область видимости из файла {file}';"+
                                $"en = 'Load module ={moduleName}= in to context from file {file}'")    
                );
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
            var manager = _engine.GlobalsManager.GetInstance<TemplateStorage>();
            manager.RegisterTemplate(file, name, kind);
        }

        public ExternalLibraryDef ProcessLibrary(string libraryPath)
        {
            bool success;
            _delayLoadedScripts.Clear();
            
            if(!_customized)
            {
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Использую НЕ кастомизированный загрузчик пакетов по умолчанию для библиотеки {libraryPath}';"+
                                $"en = 'Use NOT customized package loader for library {libraryPath}'")    
                );

                success = DefaultProcessing(libraryPath);
            }
            else
            {
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Использую КАСТОМИЗИРОВАННЫЙ загрузчик пакетов для библиотеки {libraryPath}';"+
                                $"en = 'Use CUSTOMIZED package loader for library {libraryPath}'")
                );

                success = CustomizedProcessing(libraryPath);
            }

            if (!success)
                return default;
            
            
            var library = new ExternalLibraryDef(Path.GetFileName(libraryPath));
            CompileDelayedModules(library);

            return library;
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
                .Where(x => Utils.IsValidIdentifier(x.Name))
                .ToList();

            bool hasFiles = false;

            TraceLoadLibrary(
                Locale.NStr($"ru = 'Обнаружено {files.Count} модулей в библиотеке {libraryPath}';"+
                            $"en = 'Found {files.Count} modules in library {libraryPath}'")    
            );

            foreach (var file in files)
            {
                TraceLoadLibrary(
                    Locale.NStr($"ru = 'Загружаю модуль библиотеки из {file.Path}';"+
                                $"en = 'Load library module from {file.Path}'")    
                );
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
                    library.AddModule(scriptFile.identifier, scriptFile.path);
                }
            }

            library.Modules.ForEach(moduleFile =>
            {
                var module = CompileFile(moduleFile.FilePath);
                moduleFile.Module = module;
            });
            
            library.Classes.ForEach(classFile =>
            {
                var module = CompileFile(classFile.FilePath);
                _engine.AttachedScriptsFactory.RegisterTypeModule(classFile.Symbol, module);
                classFile.Module = module;
            });

            _env.InitExternalLibrary(_engine, library);
        }

        private IExecutableModule CompileFile(string path)
        {
            var compiler = _engine.GetCompilerService();
            
            var source = _engine.Loader.FromFile(path);
            var module = _engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, source, null);

            return module;
        }

        private static Lazy<bool> TraceEnabled =
            new Lazy<bool>(() => System.Environment.GetEnvironmentVariable("OS_LRE_TRACE") == "1");
        
        public static void TraceLoadLibrary(string message)
        {
            if (TraceEnabled.Value) {
                SystemLogger.Write("LRE: " + message);
            }
        }
    }
}
