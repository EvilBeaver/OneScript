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
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Libraries;

namespace ScriptEngine.HostedScript
{
    public class LibraryLoader : AutoScriptDrivenObject<LibraryLoader>
    {
        private readonly IRuntimeEnvironment _env;
        private readonly ILibraryManager _libManager;
        private readonly ScriptingEngine _engine;

        private readonly bool _customized;
        private readonly Stack<LibraryLoadingContext> _librariesInProgress = new Stack<LibraryLoadingContext>();

        private struct DelayLoadedScriptData
        {
            public string path;
            public string identifier;
            public bool asClass;
        }

        private class LibraryLoadingContext
        {
            public LibraryLoadingContext(ExternalLibraryInfo library)
            {
                this.Library = library;
            }

            public readonly ExternalLibraryInfo Library;
            public readonly List<DelayLoadedScriptData> delayLoadedScripts = new List<DelayLoadedScriptData>();
        }
        
        private LibraryLoader(IExecutableModule moduleHandle,
            IRuntimeEnvironment env,
            ILibraryManager libManager,
            ScriptingEngine engine, IBslProcess process): base(moduleHandle)
        {
            _env = env;
            _libManager = libManager;
            _engine = engine;
            _customized = true;
            
            _engine.InitializeSDO(this, process);

        }

        private LibraryLoader(IRuntimeEnvironment env,
            ILibraryManager libManager,
            ScriptingEngine engine)
        {
            _env = env;
            _libManager = libManager;
            _engine = engine;
            _customized = false;
        }
        
        #region Static part
        
        public static LibraryLoader Create(ScriptingEngine engine, string processingScript, IBslProcess process)
        {
            var compiler = engine.GetCompilerService();
            var code = engine.Loader.FromFile(processingScript);
            var module = CompileModule(compiler, code, typeof(LibraryLoader), process);
            
            return new LibraryLoader(module, engine.Environment, engine.LibraryManager, engine, process);
        }

        public static LibraryLoader Create(ScriptingEngine engine, IBslProcess process)
        {
            return new LibraryLoader(engine.Environment, engine.LibraryManager, engine);
        }

        #endregion

        [ContextMethod("ДобавитьКласс","AddClass")]
        public void AddClass(string file, string className)
        {
            if (!Utils.IsValidIdentifier(className))
                throw RuntimeException.InvalidArgumentValue();

            _librariesInProgress.Peek().delayLoadedScripts.Add(new DelayLoadedScriptData()
                {
                    path = file,
                    identifier = className,
                    asClass = true
                });
        }

        [ContextMethod("ДобавитьМодуль", "AddModule")]
        public void AddModule(IBslProcess process, string file, string moduleName)
        {
            if (!Utils.IsValidIdentifier(moduleName))
                throw RuntimeException.InvalidArgumentValue();

            _librariesInProgress.Peek().delayLoadedScripts.Add(new DelayLoadedScriptData()
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
                
                _env.InjectGlobalProperty(BslUndefinedValue.Instance, moduleName, _librariesInProgress.Peek().Library.Package);
            }
            catch (InvalidOperationException e)
	        {
                // символ уже определен
                throw new RuntimeException($"Невозможно загрузить модуль {moduleName}. Такой символ уже определен.", e);
            }
        }

        [ContextMethod("ЗагрузитьБиблиотеку", "LoadLibrary")]
        public void LoadLibrary(string dllPath)
        {
            var context = new ComponentLoadingContext(dllPath);
            var assembly = context.LoadFromAssemblyPath(dllPath);
            _engine.AttachExternalAssembly(assembly, _env);
        }

        [ContextMethod("ДобавитьМакет", "AddTemplate")]
        public void AddTemplate(string file, string name, TemplateKind kind = TemplateKind.File)
        {
            var manager = _engine.GlobalsManager.GetInstance<TemplateStorage>();
            manager.RegisterTemplate(file, name, kind);
        }

        public PackageInfo ProcessLibrary(string libraryPath, IBslProcess process)
        {
            var package = new PackageInfo(libraryPath, Path.GetFileName(libraryPath));
            var library = new ExternalLibraryInfo(package);
            _librariesInProgress.Push(new LibraryLoadingContext(library));
            try
            {
                bool success;
                if(!_customized)
                {
                    TraceLoadLibrary(
                        Locale.NStr($"ru = 'Использую НЕ кастомизированный загрузчик пакетов по умолчанию для библиотеки {libraryPath}';"+
                                    $"en = 'Use NOT customized package loader for library {libraryPath}'")    
                    );

                    success = DefaultProcessing(libraryPath, process);
                }
                else
                {
                    TraceLoadLibrary(
                        Locale.NStr($"ru = 'Использую КАСТОМИЗИРОВАННЫЙ загрузчик пакетов для библиотеки {libraryPath}';"+
                                    $"en = 'Use CUSTOMIZED package loader for library {libraryPath}'")
                    );

                    success = CustomizedProcessing(libraryPath, process);
                }

                if (!success)
                    return default;
            
                CompileDelayedModules(library, process);
                
                return package;
            }
            finally
            {
                _librariesInProgress.Pop();
            }
        }

        private bool CustomizedProcessing(string libraryPath, IBslProcess process)
        {
            var libPathValue = ValueFactory.Create(libraryPath);
            var defaultLoading = Variable.Create(ValueFactory.Create(true), "$internalDefaultLoading");
            var cancelLoading = Variable.Create(ValueFactory.Create(false), "$internalCancelLoading");

            int eventIdx = GetScriptMethod("ПриЗагрузкеБиблиотеки", "OnLibraryLoad");
            if(eventIdx == -1)
            {
                return DefaultProcessing(libraryPath, process);
            }

            CallScriptMethod(eventIdx, new[] { libPathValue, defaultLoading, cancelLoading }, process);

            if (cancelLoading.AsBoolean()) // Отказ = Ложь
                return false;

            if (defaultLoading.AsBoolean())
                return DefaultProcessing(libraryPath, process);

            return true;

        }

        private bool DefaultProcessing(string libraryPath, IBslProcess process)
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
                AddModule(process, file.Path, file.Name);
            }

            return hasFiles;
        }

        private void CompileDelayedModules(ExternalLibraryInfo library, IBslProcess process)
        {
            foreach (var scriptFile in _librariesInProgress.Peek().delayLoadedScripts)
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

            foreach (var moduleFile in library.Modules)
            {
                moduleFile.Module = CompileFile(moduleFile.FilePath, library.Package.Id, process);
            }
            
            foreach (var classFile in library.Classes)
            {
                var module = CompileFile(classFile.FilePath, library.Package.Id, process);
                _engine.AttachedScriptsFactory.RegisterTypeModule(classFile.Symbol, module);
                classFile.Module = module;
            }

            _libManager.InitExternalLibrary(_engine, library, process);
        }

        private IExecutableModule CompileFile(string path, string ownerPackageId, IBslProcess process)
        {
            var compiler = _engine.GetCompilerService();
            
            var source = _engine.Loader.FromFile(path, ownerPackageId);
            var module = _engine.AttachedScriptsFactory.CompileModuleFromSource(compiler, source, null, process);

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
