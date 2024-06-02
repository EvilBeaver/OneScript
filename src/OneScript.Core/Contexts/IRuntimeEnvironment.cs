/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Compilation.Binding;
using ScriptEngine.Machine;

namespace OneScript.Contexts
{
    /// <summary>
    /// Программное окружение BSL-процесса. То, что "видит" скриптовый код.
    /// Используется для инжекта внешних символов в процесс.
    /// </summary>
    public interface IRuntimeEnvironment
    {
        /// <summary>
        /// Инжект объекта в глобальное пространство имен. Методы и свойства объекта станут глобальными методами и свойствами.
        /// </summary>
        /// <param name="context"></param>
        void InjectObject(IAttachableContext context);
        
        /// <summary>
        /// Инжект глобального свойства
        /// </summary>
        /// <param name="value">Значение свойства</param>
        /// <param name="identifier">Идентификатор</param>
        /// <param name="alias">Псевдоним</param>
        /// <param name="readOnly">Флаг доступности свойства только для чтения</param>
        void InjectGlobalProperty(IValue value, string identifier, string alias, bool readOnly);
        
        /// <summary>
        /// Инжект глобального свойства
        /// </summary>
        /// <param name="value">Значение свойства</param>
        /// <param name="identifier">Идентификатор</param>
        /// <param name="readOnly">Флаг доступности свойства только для чтения</param>
        void InjectGlobalProperty(IValue value, string identifier, bool readOnly);
        
        /// <summary>
        /// Установить новое значение глобального свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <param name="value">Значение свойства</param>
        void SetGlobalProperty(string propertyName, IValue value);
        
        /// <summary>
        /// Прочитать значение глобального свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        /// <returns></returns>
        IValue GetGlobalProperty(string propertyName);

        /// <summary>
        /// Получить таблицу символов для видимого контекста
        /// </summary>
        SymbolTable GetSymbolTable();
        
        /// <summary>
        /// Список подключенных внешних контекстов (слоев), доступных всегда в рамках данного окружения
        /// </summary>
        IReadOnlyCollection<IAttachableContext> AttachedContexts { get; }
    }
}