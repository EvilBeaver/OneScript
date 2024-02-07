/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.Indexes
{
    public interface IIndexCollectionSource : ICollectionContext<PropertyNameIndexAccessor>
    {
        /// <summary>
        /// Возвращает имя поля
        /// </summary>
        /// <param name="field">Поле</param>
        /// <returns>Строка. Имя поля</returns>
        string GetName(IValue field);
        
        /// <summary>
        /// Возвращает поле по имени.
        /// </summary>
        /// <param name="name">Имя поля</param>
        /// <returns>Поле</returns>
        IValue GetField(string name);
    }
}