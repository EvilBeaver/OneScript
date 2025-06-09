/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Execution
{
    /// <summary>
    /// Фабрика запуска новых bsl-потоков внутри потока C#
    /// </summary>
    public interface IBslProcessFactory
    {
        // Создать новый bsl-процесс с пустым стеком вызовов
        IBslProcess NewProcess();
    }
}