/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using OneScript.Commons;
using OneScript.Exceptions;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.Json
{
    internal static class JSONDateWriter
    {
        internal static string FormatDateForJson(DateTime date)
        {
            return FormatISODate(DropSubsecond(date));
        }

        public static string Write(IValue dateValue, JSONDateFormatEnum format, JSONDateWritingVariantEnum dateWritingVariant)
        {
            var date = DropSubsecond(GetDateArgument(dateValue));

            switch (format)
            {
                case JSONDateFormatEnum.ISO:
                    return WriteISODate(date, dateWritingVariant);

                case JSONDateFormatEnum.JavaScript:
                    CheckUniversalDateWritingVariant(format, dateWritingVariant);
                    return $"new Date({UnixMilliseconds(ConvertLocalTimeToUtc(date))})";

                case JSONDateFormatEnum.Microsoft:
                    CheckUniversalDateWritingVariant(format, dateWritingVariant);
                    return $"/Date({UnixMilliseconds(ConvertLocalTimeToUtc(date))})/";

                default:
                    throw RuntimeException.InvalidNthArgumentValue(2);
            }
        }

        private static DateTime GetDateArgument(IValue dateValue)
        {
            if (dateValue?.SystemType != BasicTypes.Date)
            {
                throw RuntimeException.InvalidNthArgumentType(1);
            }

            return dateValue.AsDate();
        }

        private static string WriteISODate(DateTime date, JSONDateWritingVariantEnum dateWritingVariant)
        {
            switch (dateWritingVariant)
            {
                case JSONDateWritingVariantEnum.LocalDate:
                    return FormatISODate(date);

                case JSONDateWritingVariantEnum.LocalDateWithOffset:
                    return FormatISODate(date) + FormatJSONDateOffset(GetLocalUtcOffset(date));

                case JSONDateWritingVariantEnum.UniversalDate:
                    return FormatISODate(ConvertLocalTimeToUtc(date)) + "Z";

                default:
                    throw RuntimeException.InvalidNthArgumentValue(3);
            }
        }

        private static string FormatISODate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private static DateTime ConvertLocalTimeToUtc(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return DateTime.MinValue;
            }

            return TimeZoneInfo.ConvertTimeToUtc(AsUnspecified(date), TimeZoneInfo.Local);
        }

        private static TimeSpan GetLocalUtcOffset(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return TimeSpan.Zero;
            }

            return TimeZoneInfo.Local.GetUtcOffset(AsUnspecified(date));
        }

        private static DateTime AsUnspecified(DateTime date)
        {
            return DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
        }

        private static DateTime DropSubsecond(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
        }

        private static long UnixMilliseconds(DateTime utcDate)
        {
            var utc = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
            return new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        }

        private static string FormatJSONDateOffset(TimeSpan offset)
        {
            var sign = offset < TimeSpan.Zero ? "-" : "+";
            var totalMinutes = Math.Abs((int)Math.Truncate(offset.TotalMinutes));
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            return $"{sign}{hours:00}:{minutes:00}";
        }

        private static void CheckUniversalDateWritingVariant(JSONDateFormatEnum format, JSONDateWritingVariantEnum dateWritingVariant)
        {
            if (dateWritingVariant == JSONDateWritingVariantEnum.UniversalDate)
            {
                return;
            }

            if (format == JSONDateFormatEnum.JavaScript)
            {
                throw new RuntimeException(Locale.NStr(
                    "ru='Невозможно сохранить локальную дату в формате JavaScript'; en='Cannot save local date in JavaScript format'"));
            }

            if (format == JSONDateFormatEnum.Microsoft)
            {
                throw new RuntimeException(Locale.NStr(
                    "ru='Невозможно сохранить локальную дату в формате Microsoft'; en='Cannot save local date in Microsoft format'"));
            }

            throw RuntimeException.InvalidNthArgumentValue(2);
        }
    }
}
