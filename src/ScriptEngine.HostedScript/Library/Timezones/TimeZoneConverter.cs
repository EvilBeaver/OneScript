/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.HostedScript.Library.Timezones
{
    public class TimeZoneConverter
    {
        public static TimeSpan GetTimespan(string timezone)
        {
            TimeSpan span;

            if (string.IsNullOrEmpty(timezone))
                span = TimeZoneInfo.Local.BaseUtcOffset;
            else if (timezone.StartsWith("GMT", StringComparison.InvariantCultureIgnoreCase))
                span = TimeSpanByGMTString(timezone);
            else
                span = TimeZoneById(timezone).BaseUtcOffset;

            return span;
        }

        public static TimeZoneInfo TimeZoneById(string id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        public static DateTime ToUniversalTime(DateTime dt, string timeZone = null)
        {
            var src = GetTimespan(timeZone);
            var dest = TimeZoneInfo.Utc.BaseUtcOffset;
            var span = dest.Subtract(src);
            return dt.Add(span);
        }

        public static DateTime ToLocalTime(DateTime dt, string timeZone = null)
        {
            var dest = GetTimespan(timeZone);
            var src = TimeZoneInfo.Utc.BaseUtcOffset;
            var span = dest.Subtract(src);
            return dt.Add(span);
        }

        public static int StandardTimeOffset(string timeZone = null, DateTime? dt = null)
        {
            var dest = GetTimespan(timeZone);

            if (dt == null)
                dt = DateTime.UtcNow;

            var local = dt.Value.Add(dest);

            var offset = local - dt.Value;

            return (int)offset.TotalSeconds;
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

        public static string TimeZonePresentation(string id)
        {
            var dt = TimeZoneById(id);
            var oprt = dt.BaseUtcOffset.Hours >= 0 ? "+" : "-";
            var result = $"GMT{oprt}{dt.BaseUtcOffset.ToString(@"hh\:mm")}";
            return result;
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

            if (!positiveOffset)
                hours = -hours;

            var span = new TimeSpan(hours, minutes, 0);

            return span;
        }

    }

}
