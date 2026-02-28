/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Values;

namespace OneScript.Contexts.Converters
{
    /// <summary>
    /// Контракт конвертера произвольного CLR-значения в BslValue и обратно.
    /// Реализующий тип должен быть обычным sealed-классом (не static) со статическими методами.
    /// Экземпляр конвертера никогда не создаётся.
    /// </summary>
    public interface IBslValueConverter
    {
        /// <summary>
        /// Превращает объект CLR в BslValue
        /// </summary>
        /// <param name="value">значение для обёртки в BslValue</param>
        /// <returns>Значение Bsl-машины</returns>
        static abstract BslValue ToBslValue(object value);

        /// <summary>
        /// Превращает значение Bsl в стандартный объект CLR
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns>Объект CLR</returns>
        static abstract object ToClrValue(BslValue value);
    }
}
