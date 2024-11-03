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
using OneScript.Exceptions;
using OneScript.Execution;
using ScriptEngine.Libraries;

namespace ScriptEngine.HostedScript
{
    public class LibraryLoader : AutoScriptDrivenObject<LibraryLoader>
    {
        private readonly ILibraryManager _libManager;
        private readonly ScriptingEngine _engine;

        private readonly bool _customized;
        private ExternalLibraryDef _library;
        
        
        private LibraryLoader(
            IExecutableModule moduleHandle,
            ILibraryManager libManager,
            ScriptingEngine engine): base(moduleHandle)
        {
            _libManager = libManager;
            _engine = engine;
            _customized = true;
            
            _engine.InitializeSDO(this);

        }

        private LibraryLoader(
            ILibraryManager libManager,
            ScriptingEngine engine)
        {
            _libManager = libManager;
            _engine = engine;
            _customized = false;
        }
        
        #region Static part
        
        public static LibraryLoader Create(ScriptingEngine engine, string processingScript)
        {
            var compiler = engine.GetCompilerService();
            var code = engine.Loader.FromFile(processingScript);
            var module = CompileModule(compiler, code, typeof(LibraryLoader));
            
            return new LibraryLoader(module, engine.LibraryManager, engine);

        }

        public static LibraryLoader Create(ScriptingEngine engine)
        {
            return new LibraryLoader(engine.LibraryManager, engine);
        }

        #endregion

        [ContextMethod("ДобавитьКласс","AddClass")]
        public void AddClass(string file, string className)
        {
            if (!Utils.IsValidIdentifier(className))
                throw RuntimeException.InvalidArgumentValue();

            _library.AddClass(className, file);
        }

        [ContextMethod("ДобавитьМодуль", "AddModule")]
        public void AddModule(string file, string moduleName)
        {
            if (!Utils.IsValidIdentifier(moduleName))
                throw RuntimeException.InvalidArgumentValue();

            _library.AddModule(moduleName, file);

         //    try
         //    {
         //        TraceLoadLibrary(
         //            Locale.NStr($"ru = 'Загружаю модуль ={moduleName}= в область видимости из файла {file}';"+
         //                        $"en = 'Load module ={moduleName}= in to context from file {file}'")    
         //        );
         //        _env.InjectGlobalProperty(null, moduleName, true);
         //        MachineInstance.Current.UpdateGlobals();
         //    }
         //    catch (InvalidOperationException e)
	        // {
         //        // символ уже определен
         //        throw new RuntimeException(String.Format("Невозможно загрузить модуль {0}. Такой символ уже определен.", moduleName), e);
         //    }
        }

        [ContextMethod("ЗагрузитьБиблиотеку", "LoadLibrary")]
        public void LoadLibrary(string dllPath)
        {
            var context = new ComponentLoadingContext(dllPath);
            var assembly = context.LoadFromAssemblyPath(dllPath);
            _engine.AttachExternalAssembly(assembly);
        }

        [ContextMethod("ДобавитьМакет", "AddTemplate")]
        public void AddTemplate(string file, string name, TemplateKind kind = TemplateKind.File)
        {
            var manager = _engine.GlobalsManager.GetInstance<TemplateStorage>();
            manager.RegisterTemplate(file, name, kind);
        }

        public bool ProcessLibrary(string libraryPath)
        {
            bool success;
            _library = new ExternalLibraryDef(Path.GetFileName(libraryPath));
            
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
            
            _libManager.InitExternalLibrary(_engine, _library);

            return true;
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

        private static Lazy<bool> TraceEnabled =
            new Lazy<bool>(() => Environment.GetEnvironmentVariable("OS_LRE_TRACE") == "1");

        private static void TraceLoadLibrary(string message)
        {
            if (TraceEnabled.Value) {
                SystemLogger.Write("LRE: " + message);
            }
        }
    }
}
