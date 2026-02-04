/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.DebugProtocol;

namespace VSCode.DebugAdapter
{
    /// <summary>
    /// Провайдер переменных для конкретного scope или вложенного объекта.
    /// Не хранит данные - только знает как их получить из рантайма.
    /// </summary>
    public interface IVariablesProvider
    {
        /// <summary>
        /// Получить переменные из рантайма через debug service
        /// </summary>
        Variable[] FetchVariables(IDebuggerService service);
        
        /// <summary>
        /// Создать провайдер для дочерних элементов переменной по индексу
        /// </summary>
        IVariablesProvider CreateChildProvider(int variableIndex);
    }
}

