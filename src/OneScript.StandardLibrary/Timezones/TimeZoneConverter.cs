/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace OneScript.StandardLibrary.Timezones
{
    public class TimeZoneConverter
    {
        private const int MAX_HOURS = 23;
        private const int MAX_MINUTES = 59;

        public static TimeSpan GetTimespan(string timezone)
        {
            var tz = ResolveTimeZone(timezone);
            return tz.BaseUtcOffset;
        }

        public static TimeZoneInfo TimeZoneById(string id)
        {
            return ResolveTimeZone(id);
        }

        public static DateTime ToUniversalTime(DateTime dt, string timeZone = null)
        {
            var tz = ResolveTimeZone(timeZone);
            var unspecified = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, tz);
        }

        public static DateTime ToLocalTime(DateTime dt, string timeZone = null)
        {
            var tz = ResolveTimeZone(timeZone);
            var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
        }

        public static int StandardTimeOffset(string timeZone = null, DateTime? dt = null)
        {
            var tz = ResolveTimeZone(timeZone);
            return (int)tz.BaseUtcOffset.TotalSeconds;
        }

        public static IEnumerable<string> GetAvailableTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones()
                .Select(x => x.Id);
        }

        public static string TimeZone()
        {
            return TimeZoneInfo.Local.Id;
        }

        public static string TimeZonePresentation(string timeZone)
        {
            TimeSpan offset;
            if (IsGmtString(timeZone))
            {
                offset = TimeSpanByGMTString(timeZone);
            }
            else
            {
                var tz = ResolveTimeZone(timeZone);
                offset = tz.BaseUtcOffset;
            }
            return FormatGmtOffset(offset);
        }

        private static TimeSpan TimeSpanByGMTString(string gmtString)
        {

            gmtString = gmtString.ToLower();

            var positiveOffset = gmtString.StartsWith("gmt+");

            var arr_id = gmtString.Split(
                new string[] { "+", ":", "-" },
                StringSplitOptions.None);

            int hours = 0;
            int minutes = 0;

            if (arr_id.Length < 2 || arr_id.Length > 3)
                throw new TimeZoneNotFoundException();
            else if (arr_id.Length == 3)
                minutes = int.Parse(arr_id[2]);

            hours = int.Parse(arr_id[1]);

            if (hours > MAX_HOURS || minutes > MAX_MINUTES)
                throw new TimeZoneNotFoundException();

            if (!positiveOffset)
                hours = -hours;

            var span = new TimeSpan(hours, minutes, 0);

            return span;
        }

        private static bool IsGmtString(string zone)
        {
            return !string.IsNullOrEmpty(zone) && zone.StartsWith("GMT", StringComparison.InvariantCultureIgnoreCase);
        }

        private static string FormatGmtOffset(TimeSpan offset)
        {
            var sign = offset >= TimeSpan.Zero ? "+" : "-";
            var d = offset.Duration();
            return $"GMT{sign}{d.ToString(@"hh\:mm")}";
        }

        private static TimeZoneInfo ResolveTimeZone(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return TimeZoneInfo.Local;

            if (IsGmtString(id))
            {
                var offset = TimeSpanByGMTString(id);
                var display = FormatGmtOffset(offset);
                return TimeZoneInfo.CreateCustomTimeZone(id, offset, display, display);
            }

            // Имена таймзон в Windows отличаются от IANA и будут не совпадать
            // с поведением 1С, работающей с ICU 
            // Надо заводить отдельную карту для коротких имен таймзон.
            // Как минимум, для EET, CET, WET и может еще чего-то.
            // Например, зона "E. Europe Standard Time" работает в .net только на Windows.
            // База данных IANA (https://www.iana.org/time-zones) в принципе не содержит этих таймзон, но вероятно
            // имеет смысл их поддержать, т.к. в 1С они используются, и GetAvailableTimeZones() их возвращает.
            
            // TODO: Переписать на нормальное решение
            var tzFixes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            tzFixes.Add("EET", "GMT+02:00");
            tzFixes.Add("CET", "GMT+01:00");
            
            if (tzFixes.TryGetValue(id, out var gmtString))
            {
                return ResolveTimeZone(gmtString);
            }

            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
    }

}
