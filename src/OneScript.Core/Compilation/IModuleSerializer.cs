/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Execution;

namespace OneScript.Compilation
{
    /// <summary>
    /// Интерфейс для сериализации исполняемых модулей
    /// </summary>
    public interface IModuleSerializer
    {
        /// <summary>
        /// Сериализовать модуль в поток
        /// </summary>
        void Serialize(IExecutableModule module, System.IO.Stream stream);
        
        /// <summary>
        /// Десериализовать модуль из потока
        /// </summary>
        IExecutableModule Deserialize(System.IO.Stream stream);
        
        /// <summary>
        /// Проверить, может ли сериализатор работать с данным типом модуля
        /// </summary>
        bool CanSerialize(IExecutableModule module);
    }
}