/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    /// <summary>
    /// Глобальный менеджер сбора статистики исполнения кода.
    /// </summary>
    [ContextClass("СборСтатистики", "CodeStatistics")]
    public sealed class CodeStatisticsContext : AutoContext<CodeStatisticsContext>
    {
        [ContextMethod("СборДоступен", "CollectionAvailable")]
        public bool CollectionAvailable(IBslProcess process)
        {
            return process.Services.TryResolve<ICodeStatCollector>() != null;
        }

        [ContextMethod("НачатьСбор", "StartCollection")]
        public CodeStatisticsCollector StartCollection(IBslProcess process)
        {
            if (process.Services.TryResolve<ICodeStatCollector>() is not CodeStatHub hub)
            {
                throw new RuntimeException(
                    "Сбор статистики кода не включён. Запустите приложение с параметром -codestat",
                    "Code statistics collection is not enabled. Start the application with the -codestat switch");
            }

            var session = hub.StartSession();
            return new CodeStatisticsCollector(hub, session);
        }
    }
}
