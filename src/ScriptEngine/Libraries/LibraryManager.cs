/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;
using OneScript.Execution;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Менеджер загрузки внешних библиотек
    /// </summary>
    internal class LibraryManager : ILibraryManager
    {
        private readonly IRuntimeContextInstance _contextOfGlobalSymbols;

        public LibraryManager(IRuntimeContextInstance contextOfGlobalSymbols)
        {
            _contextOfGlobalSymbols = contextOfGlobalSymbols;
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryInfo library, IBslProcess process)
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

            loadedObjects.ForEach(sdo => runtime.InitializeSDO(sdo, process));
        }
    }
}
