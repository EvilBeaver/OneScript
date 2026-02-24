/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Execution;
using OneScript.Values;

namespace OneScript.Contexts.Converters
{
    /// <summary>
    /// Базовый класс конвертации произвольного значения в BslValue и обратно
    /// </summary>
    public interface IBslValueConverter
    {
        /// <summary>
        /// Превращает объект CLR в BslValue
        /// </summary>
        /// <param name="value">значение для обертки в BslValue</param>
        /// <param name="defaultConverter">Стандартный системный конвертер. Можно использовать, как сервис конвертации</param>
        /// <param name="process">Текущий bsl-процесс</param>
        /// <returns>Значение Bsl-машины</returns>
        public BslValue ToBslValue(object value, IBslValueConverter defaultConverter, IBslProcess process);
        
        /// <summary>
        /// Превращает значение Bsl в стандартный объект Clr
        /// </summary>
        /// <param name="value">Значение</param>
        /// <param name="defaultConverter">Стандартный системный конвертер. Можно использовать, как сервис конвертации</param>
        /// <param name="process">Текущий bsl-процесс</param>
        /// <returns>Объект CLR</returns>
        public object ToClrValue(BslValue value, IBslValueConverter defaultConverter, IBslProcess process);
    }
}