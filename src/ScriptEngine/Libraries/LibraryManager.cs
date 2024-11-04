/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Execution;
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
        }
        
        private void CompileDelayedModules(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var ownerContext = new ModulesOrderingContext();

            // Зарегистрируем модули, как видимые символы
            foreach (var module in library.Modules)
            {
                ownerContext.AddKnownModule(module);
            }
            runtime.Environment.InjectObject(ownerContext);
            MachineInstance.Current.UpdateGlobals();
            
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
