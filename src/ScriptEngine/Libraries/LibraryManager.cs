using OneScript.Commons;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneScript.Compilation.Binding;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Localization;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Временная имплементация временного интерфейса ILibraryManager
    /// Нужна для откусывания ответственностей от RuntimeEnvironment
    /// </summary>
    internal class LibraryManager : ILibraryManager
    {
        private readonly IRuntimeContextInstance _contextOfGlobalSymbols;

        public LibraryManager(IRuntimeContextInstance contextOfGlobalSymbols)
        {
            _contextOfGlobalSymbols = contextOfGlobalSymbols;
        }
        
        public LibraryManager()
        {
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            CompileDelayedModules(runtime, library);
            
        }
        
        private void CompileDelayedModules(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var symbols = runtime.Environment.GetSymbolTable();

            // Зарегистрируем все символы модулей из данной библиотеки
            // Попутно проверяем конфликт имен среди известных символов
            var libraryScope = new SymbolScope();
            int i = 0;
            foreach (var module in library.Modules)
            {
                if (symbols.FindVariable(module.Symbol, out _))
                {
                    // символ уже определен
                    throw new RuntimeException(
                        new BilingualString(
                            $"Невозможно загрузить модуль {module.Symbol}. Такой символ уже определен.",
                            $"Unable to load module {module.Symbol}. Symbol is already defined.")
                    );
                }

                libraryScope.DefineVariable(
                    BslPropertyBuilder.Create()
                        .Name(module.Symbol)
                        .CanRead(true)
                        .CanWrite(false)
                        .SetDispatchingIndex(i++)
                        .Build().ToSymbol()
                );
            }
            
            // Получим байткоды всех классов и модулей из библиотеки
            var ownerContext = new ModulesOrderingContext(libraryScope);
            symbols.PushScope(libraryScope, new ModulesOrderingContext(libraryScope));
            
            library.Modules.ForEach(moduleFile =>
            {
                var module = CompileFile(runtime, moduleFile.FilePath);
                moduleFile.Module = module;
            });
            
            library.Classes.ForEach(classFile =>
            {
                var module = CompileFile(runtime, classFile.FilePath);
                runtime.AttachedScriptsFactory.RegisterTypeModule(classFile.Symbol, module);
                classFile.Module = module;
            });

            // Проведем инициализацию всех модулей
            foreach (var module in library.Modules)
            {
                var instance = runtime.CreateUninitializedSDO(module.Module);

                ownerContext.SetUninitializedInstance(module, instance);
            }

            ownerContext.InitializeModules(runtime);
        }

        private IExecutableModule CompileFile(ScriptingEngine runtime, string path)
        {
            var compiler = runtime.GetCompilerService();
            
            var source = runtime.Loader.FromFile(path);
            var module = runtime.AttachedScriptsFactory.CompileModuleFromSource(compiler, source, null);

            return module;
        }
    }
}
