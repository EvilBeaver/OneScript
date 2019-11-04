/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;

namespace ScriptEngine.HostedScript.Library.TimeSpan
{
    [ContextClass("ИнтервалВремени", "TimeSpan")]
    public class TimeSpan : AutoContext<TimeSpan>
    {
        public TimeSpan(decimal milliseconds)
        {
            SystemTimeSpan = System.TimeSpan.FromMilliseconds((double)milliseconds);
        }

        public TimeSpan(DateTime startDate, DateTime endDate)
        {
            SystemTimeSpan = endDate - startDate;
        }

        [ScriptConstructor(Name = "Из миллисекунд")]
        public static TimeSpan Constructor(IValue milliseconds)
        {
            return new TimeSpan(milliseconds.AsNumber());
        }

        [ScriptConstructor(Name = "Из разности двух дат")]
        public static TimeSpan Constructor(IValue startDate, IValue endDate)
        {
            return new TimeSpan(startDate.AsDate(), endDate.AsDate());
        }

        /// <summary>
        /// Возвращает количество дней периода времени.
        /// </summary>
        [ContextProperty("Дни")]
        public int Days
        {
            get { return SystemTimeSpan.Days; }
        }

        /// <summary>
        /// Возвращает количество часов периода времени.
        /// </summary>
        [ContextProperty("Часы")]
        public int Hours
        {
            get { return SystemTimeSpan.Hours; }
        }

        /// <summary>
        /// Возвращает количество миллисекунд периода времени.
        /// </summary>
        [ContextProperty("Миллисекунды")]
        public int Milliseconds
        {
            get { return SystemTimeSpan.Milliseconds; }
        }

        /// <summary>
        /// Возвращает количество минут периода времени.
        /// </summary>
        [ContextProperty("Минуты")]
        public int Minutes
        {
            get { return SystemTimeSpan.Minutes; }
        }

        /// <summary>
        /// Возвращает количество секунд периода времени.
        /// </summary>
        [ContextProperty("Секунды")]
        public int Seconds
        {
            get { return SystemTimeSpan.Seconds; }
        }

        /// <summary>
        /// Возвращает общее количество дней периода времени (возможно, дробное).
        /// </summary>
        [ContextProperty("ВсегоДней")]
        public double TotalDays
        {
            get { return SystemTimeSpan.TotalDays; }
        }

        /// <summary>
        /// Возвращает общее количество часов периода времени (возможно, дробное).
        /// </summary>
        [ContextProperty("ВсегоЧасов")]
        public double TotalHours
        {
            get { return SystemTimeSpan.TotalHours; }
        }

        /// <summary>
        /// Возвращает общее количество миллисекунд периода времени (возможно, дробное).
        /// </summary>
        [ContextProperty("ВсегоМиллисекунд")]
        public double TotalMilliseconds
        {
            get { return SystemTimeSpan.TotalMilliseconds; }
        }

        /// <summary>
        /// Возвращает общее количество минут периода времени (возможно, дробное).
        /// </summary>
        [ContextProperty("ВсегоМинут")]
        public double TotalMinutes
        {
            get { return SystemTimeSpan.TotalMinutes; }
        }

        /// <summary>
        /// Возвращает общее количество секунд периода времени (возможно, дробное).
        /// </summary>
        [ContextProperty("ВсегоСекунд")]
        public double TotalSeconds
        {
            get { return SystemTimeSpan.TotalSeconds; }
        }

        public System.TimeSpan SystemTimeSpan { get; set; }

        public override string AsString()
        {
            return SystemTimeSpan.ToString();
        }
    }
}
