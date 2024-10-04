using OneScript.Commons;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Временная имплементация временного интерфейса ILibraryManager
    /// Нужна для откусывания ответственностей от RuntimeEnvironment
    /// </summary>
    internal class LibraryManager : ILibraryManager
    {
        private readonly List<ExternalLibraryDef> _externalLibs = new List<ExternalLibraryDef>();
        private readonly IRuntimeContextInstance _contextOfGlobalSymbols;

        public LibraryManager(IRuntimeContextInstance contextOfGlobalSymbols)
        {
            _contextOfGlobalSymbols = contextOfGlobalSymbols;
        }

        public IEnumerable<ExternalLibraryDef> GetLibraries()
        {
            return _externalLibs.ToArray();
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var loadedObjects = new ScriptDrivenObject[library.Modules.Count];
            int i = 0;
            foreach (var module in library.Modules)
            {
                var instance = runtime.CreateUninitializedSDO(module.Module);

                var propId = _contextOfGlobalSymbols.GetPropertyNumber(module.Symbol);
                _contextOfGlobalSymbols.SetPropValue(propId, instance);
                loadedObjects[i++] = instance;
            }

            _externalLibs.Add(library);
            loadedObjects.ForEach(runtime.InitializeSDO);
        }
    }
}
