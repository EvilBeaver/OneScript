/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using OneScript.Commons;
using ScriptEngine.Machine;
using OneScript.Execution;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Менеджер загрузки внешних библиотек
    /// </summary>
    internal class LibraryManager : ILibraryManager
    {
        private readonly PropertyBag _contextOfGlobalSymbols;

        public LibraryManager(PropertyBag contextOfGlobalSymbols)
        {
            _contextOfGlobalSymbols = contextOfGlobalSymbols;
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryInfo library, IBslProcess process)
        {
            var propIds = new int[library.Modules.Count];

            // Ошибку модуля, инициализированного при обращении, могло поймать тело другого модуля.
            // Она поднимается в очередь самого модуля, и загрузка библиотеки падает, как раньше
            var failures = new Dictionary<int, ExceptionDispatchInfo>();
            int i = 0;
            foreach (var module in library.Modules)
            {
                var instance = runtime.CreateUninitializedSDO(module.Module);

                var propId = _contextOfGlobalSymbols.GetPropertyNumber(module.Symbol);
                _contextOfGlobalSymbols.SetPropValue(propId, instance);

                // Если тело другого модуля обратится к этому раньше его очереди, модуль инициализируется при обращении
                _contextOfGlobalSymbols.SetInitializer(propId, () =>
                {
                    try
                    {
                        runtime.InitializeSDO(instance, process);
                    }
                    catch (Exception e)
                    {
                        failures[propId] = ExceptionDispatchInfo.Capture(e);
                        throw;
                    }
                });
                propIds[i++] = propId;
            }

            try
            {
                foreach (var propId in propIds)
                {
                    _contextOfGlobalSymbols.RunInitializer(propId);
                    if (failures.TryGetValue(propId, out var failure))
                        failure.Throw();
                }
            }
            finally
            {
                // Тело модуля упало - остальные модули остаются без инициализации, как и раньше
                propIds.ForEach(_contextOfGlobalSymbols.RemoveInitializer);
            }
        }
    }
}
