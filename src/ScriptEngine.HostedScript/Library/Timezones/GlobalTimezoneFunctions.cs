/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Timezones
{
    /// <summary>
    /// Глобальный контекст. Операции с временными зонами и часовыми поясами.
    /// </summary>
    [GlobalContext(Category = "Процедуры и функции взаимодействия с часовыми поясами и временными зонами")]
    public sealed class GlobalTimezoneFunctions : GlobalContextBase<GlobalTimezoneFunctions>
    {
        public static IAttachableContext CreateInstance()
        {
            return new GlobalTimezoneFunctions();
        }

        /// <summary>
        /// Получает идентификатор часового пояса компьютера.
        /// </summary>
        /// <returns>Строка - Идентификатор часового пояса компьютера</returns>
        [ContextMethod("ЧасовойПояс", "TimeZone")]
        public string TimeZone()
        {
            return TimeZoneConverter.TimeZone();
        }

        /// <summary>
        /// Получает массив строк допустимых идентификаторов часовых поясов.
        /// </summary>
        /// <returns>Массив - Доступные идентификаторы часовых поясов</returns>
        [ContextMethod("ПолучитьДопустимыеЧасовыеПояса", "GetAvailableTimeZones")]
        public ArrayImpl GetAvailableTimeZones()
        {
            return new ArrayImpl(TimeZoneConverter.GetAvailableTimeZones()
                .Select(x => ValueFactory.Create(x)));
        }

        /// <summary>
        /// Возвращает локализованное наименование часового пояса с заданным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор часового пояса</param>
        /// <returns>Строка</returns>
        [ContextMethod("ПредставлениеЧасовогоПояса", "TimeZonePresentation")]
        public string TimeZonePresentation(string id)
        {
            try
            {
                return TimeZoneConverter.TimeZonePresentation(id);
            }
            catch (TimeZoneNotFoundException)
            {
                throw RuntimeException.InvalidNthArgumentValue(1);
            }
        }

        /// <summary>
        /// Преобразует универсальное время в местное время заданного часового пояса.
        /// </summary>
        /// <param name="universalTime">Универсальное время.</param>
        /// <param name="timeZone">Часовой пояс. Подробнее см. синтакс-помощник от 1С.</param>
        /// <returns>Дата</returns>
        [ContextMethod("МестноеВремя", "ToLocalTime")]
        public IValue ToLocalTime(IValue universalTime, string timeZone = null)
        {
            try
            {
                var dt = TimeZoneConverter.ToLocalTime(universalTime.AsDate(), timeZone);
                return ValueFactory.Create(dt);
            }
            catch (TimeZoneNotFoundException)
            {
                throw RuntimeException.InvalidNthArgumentValue(2);
            }
        }

        /// <summary>
        /// Преобразует местное время в заданном часовом поясе в универсальное время.
        /// </summary>
        /// <param name="localTime">Локальное время.</param>
        /// <param name="timeZone">Часовой пояс. Подробнее см. синтакс-помощник от 1С.</param>
        /// <returns>Дата</returns>
        [ContextMethod("УниверсальноеВремя", "ToUniversalTime")]
        public IValue ToUniversalTime(IValue localTime, string timeZone = null)
        {
            try
            {
                var dt = TimeZoneConverter.ToUniversalTime(localTime.AsDate(), timeZone);
                return ValueFactory.Create(dt);
            }
            catch (TimeZoneNotFoundException)
            {
                throw RuntimeException.InvalidNthArgumentValue(2);
            }
        }

        /// <summary>
        /// Получает смещение в секундах стандартного времени заданного часового пояса 
        /// относительно универсального времени без учета летнего времени для заданного универсального времени
        /// </summary>
        /// <param name="timeZone">Часовой пояс. Подробнее см. синтакс-помощник от 1С.</param>
        /// <param name="universalTime">Универсальное время (UTC), для которого нужно определить смещение </param>
        /// <returns>Число</returns>
        [ContextMethod("СмещениеСтандартногоВремени", "StandardTimeOffset")]
        public int StandardTimeOffset(string timeZone = null, IValue universalTime = null)
        {
            try
            {
                return TimeZoneConverter.StandardTimeOffset(timeZone, universalTime?.AsDate());
            }
            catch (TimeZoneNotFoundException)
            {
                throw RuntimeException.InvalidNthArgumentValue(1);
            }
        }
    }
}
