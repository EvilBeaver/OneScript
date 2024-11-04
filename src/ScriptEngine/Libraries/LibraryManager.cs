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
using ScriptEngine.Machine;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Временная имплементация временного интерфейса ILibraryManager
    /// Нужна для откусывания ответственностей от RuntimeEnvironment
    /// </summary>
    internal class LibraryManager : ILibraryManager
    {
        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            CompileDelayedModules(runtime, library);
            MachineInstance.Current.UpdateGlobals();
        }
        
        private void CompileDelayedModules(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var ownerContext = new ModulesOrderingContext();
            
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
            
            runtime.Environment.InjectObject(ownerContext);
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
