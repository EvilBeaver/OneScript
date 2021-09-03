/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    public static class BuiltInFunctions
    {
        //[ContextMethod("Вычислить", "Eval")]
        public static BslValue Eval(string arg)
        {
            throw new NotSupportedException();
        }

        #region String Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("СтрДлина", "StrLen")]
        public static int StrLen(string arg) => arg.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("СокрЛ", "TrimL")]
        public static string TrimL(string arg) => arg.TrimStart();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("СокрП", "TrimR")]
        public static string TrimR(string arg) => arg.TrimEnd();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("СокрЛП", "TrimAll")]
        public static string TrimLR(string arg) => arg.Trim();

        [ContextMethod("Лев", "Left")]
        public static string Left(string str, int len)
        {
            if (len > str.Length)
                len = str.Length;
            else if (len < 0)
            {
                return string.Empty;
            }

            return str.Substring(0, len);
        }

        [ContextMethod("Прав", "Right")]
        public static string Right(string str, int len)
        {
            if (len > str.Length)
                len = str.Length;
            else if (len < 0)
            {
                return string.Empty;
            }

            int startIdx = str.Length - len;
            return str.Substring(startIdx, len);
        }

        [ContextMethod("Сред", "Mid")]
        public static string Mid(string str, int start, int len = -1)
        {
            if (start < 1)
                start = 1;

            if (start+len > str.Length || len < 0)
                len = str.Length-start+1;

            string result;

            if (start > str.Length || len == 0)
            {
                result = "";
            }
            else
            {
                result = str.Substring(start - 1, len);
            }

            return result;
        }

        [ContextMethod("Найти", "Find")]
        public static int StrPos(string needle, string haystack)
        {
            var result = haystack.IndexOf(needle, StringComparison.Ordinal) + 1;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ВРег", "Upper")]
        public static string UCase(string str) => str.ToUpper();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("НРег", "Lower")]
        public static string LCase(string str) => str.ToLower();

        [ContextMethod("ТРег", "Title")]
        public static string TCase(string str)
        {
            char[] array = str.ToCharArray();
            // Handle the first letter in the string.
            bool inWord = false;
            if (array.Length >= 1)
            {
                if (char.IsLetter(array[0]))
                    inWord = true;

                if(char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            // Scan through the letters, checking for spaces.
            // ... Uppercase the lowercase letters following spaces.
            for (int i = 1; i < array.Length; i++)
            {
                if (inWord && Char.IsLetter(array[i]))
                    array[i] = Char.ToLower(array[i]);
                else if (Char.IsSeparator(array[i]) || Char.IsPunctuation(array[i]))
                    inWord = false;
                else if(!inWord && Char.IsLetter(array[i]))
                {
                    inWord = true;
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
	        
            return new string(array);
        }

        [ContextMethod("Символ", "Char")]
        public static object Chr(int code)
        {
            return new string(new char[1] { (char)code });
        }

        [ContextMethod("КодСимвола", "CharCode")]
        public static object ChrCode(string strChar, int? position = null)
        {
            if(position != null)
            {
                position -= 1;
            }
            else
            {
                position = 0;
            }
            
            int result;
            if (strChar.Length == 0)
                result = 0;
            else if (position >= 0 && position < strChar.Length)
                result = (int)strChar[(int)position];
            else
                throw RuntimeException.InvalidArgumentValue();

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ПустаяСтрока", "IsBlankString")]
        public static bool EmptyStr(string arg) => string.IsNullOrWhiteSpace(arg);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("СтрЗаменить", "StrReplace")]
        public static string StrReplace(string sourceString, string searchVal, string newVal) =>
            sourceString.Replace(searchVal, newVal);

        [ContextMethod("СтрПолучитьСтроку", "StrGetLine")]
        public static string StrGetLine(string strArg, int lineNumber)
        {
            string result = "";
            if (lineNumber >= 1)
            {
                string[] subStrVals = strArg.Split(new Char[] { '\n' }, lineNumber + 1);
                result = subStrVals[lineNumber - 1];
            }

            return result;
        }

        [ContextMethod("СтрЧислоСтрок", "StrLineCount")]
        public static int StrLineCount(string strArg)
        {
            int pos = 0;
            int lineCount = 1;
            while (pos >= 0 && pos < strArg.Length)
            {
                pos = strArg.IndexOf('\n', pos);
                if (pos >= 0)
                {
                    lineCount++;
                    pos++;
                }
            }

            return lineCount;
        }

        [ContextMethod("СтрЧислоВхождений", "StrOccurrenceCount")]
        public static int StrEntryCount(string where, string what)
        {
            var pos = where.IndexOf(what, StringComparison.CurrentCulture);
            var entryCount = 0;
            while(pos >= 0)
            {
                entryCount++;
                var nextIndex = pos + what.Length;
                if (nextIndex >= where.Length)
                    break;

                pos = where.IndexOf(what, nextIndex, StringComparison.CurrentCulture);
            }

            return entryCount;
        }
        
        #endregion

        #region Date Functions
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("Год", "Year")]
        public static int Year(DateTime date) => date.Year;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("Месяц", "Month")]
        public static int Month(DateTime date) => date.Month;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("День", "Day")]
        public static int Day(DateTime date) => date.Day;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("Час", "Hour")]
        public static int Hour(DateTime date) => date.Hour;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("Минута", "Minute")]
        public static int Minute(DateTime date) => date.Minute;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("Секунда", "Second")]
        public static int Second(DateTime date) => date.Second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("НачалоГода", "BegOfYear")]
        public static DateTime BegOfYear(DateTime date) => new DateTime(date.Year, 1, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("НачалоМесяца", "BegOfMonth")]
        public static DateTime BegOfMonth(DateTime date) => new DateTime(date.Year, date.Month, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("НачалоДня", "BegOfDay")]
        public static DateTime BegOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("НачалоЧаса", "BegOfHour")]
        public static DateTime BegOfHour(DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("НачалоМинуты", "BegOfMinute")]
        public static DateTime BegOfMinute(DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);

        [ContextMethod("НачалоКвартала", "BegOfQuarter")]
        public static DateTime BegOfQuarter(DateTime date)
        {
            //1,4,7,10
            int quarterMonth;
            if (date.Month >= 1 && date.Month <= 3)
            {
                quarterMonth = 1;
            }
            else if (date.Month >= 4 && date.Month <= 6)
            {
                quarterMonth = 4;
            }
            else if (date.Month >= 7 && date.Month <= 9)
            {
                quarterMonth = 7;
            }
            else
            {
                quarterMonth = 10;
            }
            var result = new DateTime(date.Year, quarterMonth, 1);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("КонецГода", "EndOfYear")]
        public static DateTime EndOfYear(DateTime date)
        {
            var year = date.Year;
            return new DateTime(year, 12, DateTime.DaysInMonth(year, 12), 23, 59, 59);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("КонецМесяца", "EndOfMonth")]
        public static DateTime EndOfMonth(DateTime date)
        {
            var year = date.Year;
            var month = date.Month;
            return new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("КонецДня", "EndOfDay")]
        public static DateTime EndOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("КонецЧаса", "EndOfHour")]
        public static DateTime EndOfHour(DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, date.Hour, 59, 59);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("КонецМинуты", "EndOfMinute")]
        public static DateTime EndOfMinute(DateTime date) =>
            new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 59);

        [ContextMethod("КонецКвартала", "EndOfQuarter")]
        public static DateTime EndOfQuarter(DateTime date)
        {
            //1,4,7,10
            int quarterMonth;
            if (date.Month >= 1 && date.Month <= 3)
            {
                quarterMonth = 3;
            }
            else if (date.Month >= 4 && date.Month <= 6)
            {
                quarterMonth = 6;
            }
            else if (date.Month >= 7 && date.Month <= 9)
            {
                quarterMonth = 9;
            }
            else
            {
                quarterMonth = 12;
            }
            var result = new DateTime(date.Year, quarterMonth, DateTime.DaysInMonth(date.Year, quarterMonth), 23, 59, 59);
            return result;
        }

        [ContextMethod("НеделяГода", "WeekOfYear")]
        public static int WeekOfYear(DateTime date)
        {
            var cal = new System.Globalization.GregorianCalendar();

            return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ДеньГода", "DayOfYear")]
        public static int DayOfYear(DateTime date) => date.DayOfYear;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ДеньНедели", "DayOfWeek")]
        public static int DayOfWeek(DateTime date)
        {
            var day = (int)date.DayOfWeek;
            if (day == 0)
            {
                day = 7;
            }

            return day;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ДобавитьМесяц", "AddMonth")]
        public static object AddMonth(DateTime date, int numToAdd) => date.AddMonths(numToAdd);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ТекущаяДата", "CurrentDate")] 
        public static DateTime CurrentDate()
        {
            var date = DateTime.Now;
            return date.AddTicks(-(date.Ticks % TimeSpan.TicksPerSecond));
        }
        
        #endregion

        #region Math Functions

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("Цел", "Int")]
        public static decimal Integer(decimal arg) => Math.Truncate(arg);

        [ContextMethod("Окр", "Round")]
        public static decimal Round(decimal num, int? digits = null, int? mode = null)
        {
            var digitsMath = digits??0;
            var modeMath = mode??0;
            if (modeMath != 0)
                modeMath = 1;
            
            decimal scale = (decimal)Math.Pow(10.0, digitsMath);
            decimal scaled = Math.Abs(num) * scale;

            var director = (int)((scaled - (long)scaled) * 10 % 10);

            decimal round;
            if (director == 5)
                round = Math.Floor(scaled + modeMath * 0.5m * Math.Sign(digitsMath));
            else if (director > 5)
                round = Math.Ceiling(scaled);
            else
                round = Math.Floor(scaled);
            
            decimal result;
            
            if(digits >= 0)
                result = (Math.Sign(num) * round / scale);
            else
                result = (Math.Sign(num) * round * scale);

            return result;
        }

        [ContextMethod("Log")]
        public static decimal Log(decimal num) => (decimal)Math.Log((double) num);

        [ContextMethod("Log10")]
        public static decimal Log10(decimal num) => (decimal)Math.Log10((double) num);

        [ContextMethod("Sin")]
        public static decimal Sin(decimal num) => (decimal) Math.Sin((double) num);

        [ContextMethod("Cos")]
        public static decimal Cos(decimal num) => (decimal) Math.Cos((double) num);

        [ContextMethod("Tan")]
        public static decimal Tan(decimal num) => (decimal) Math.Tan((double) num);

        [ContextMethod("ASin")]
        public static decimal ASin(decimal num) => (decimal) Math.Asin((double) num);

        [ContextMethod("ACos")]
        public static decimal ACos(decimal num) => (decimal) Math.Acos((double) num);

        [ContextMethod("ATan")]
        public static decimal ATan(decimal num) => (decimal) Math.Atan((double) num);

        [ContextMethod("Exp")]
        public static decimal Exp(decimal num) => (decimal) Math.Exp((double) num);

        [ContextMethod("Pow")]
        public static decimal Pow(decimal powBase, decimal powPower)
        {
            int exp = (int)powPower;
            decimal result;
            if (exp >= 0 && exp == powPower)
                result = PowInt(powBase, (uint)exp);
            else
                result = (decimal)Math.Pow((double)powBase, (double)powPower);

            return result;
        }

        [ContextMethod("Sqrt")]
        public static decimal Sqrt(decimal num) => (decimal) Math.Sqrt((double) num);

        [ContextMethod("Мин", "Min")]
        public static decimal Min(decimal arg1, params decimal[] args)
        {
            if (args.Length == 0)
                return arg1;
            
            var min = arg1;
            for (int i = 0; i < args.Length; i++)
            {
                var current = args[i];
                if (current.CompareTo(min) < 0)
                    min = current;
            }

            return min;
        }

        [ContextMethod("Макс", "Max")]
        public static decimal Max(decimal arg1, params decimal[] args)
        {
            if (args.Length == 0)
                return arg1;
            
            var max = arg1;
            for (int i = 0; i < args.Length; i++)
            {
                var current = args[i];
                if (current.CompareTo(max) > 0)
                    max = current;
            }

            return max;
        }

        #endregion

        #region Other
        
        // [ContextMethod("формат", "format")] public static object Format(object arg){}
        // [ContextMethod("информацияобошибке", "errorinfo")] public static object ExceptionInfo(object arg){}
        // [ContextMethod("описаниеошибки", "errordescription")] public static object ExceptionDescr(object arg){}
        // [ContextMethod("текущийсценарий", "currentscript")] public static object ModuleInfo(object arg){}

        #endregion
        

        // private void Type(int arg)
        // {
        //     var typeName = _operationStack.Pop().AsString();
        //     var type = TypeManager.GetTypeByName(typeName);
        //     var value = new BslTypeValue(type);
        //     _operationStack.Push(value);
        //     NextInstruction();
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [ContextMethod("ТипЗнч", "TypeOf")]
        public static BslValue ValType(BslValue value) => new BslTypeValue(value.SystemType);
        
        private static decimal PowInt(decimal bas, uint exp)
        {
            decimal pow = 1;

            while (true)
            {
                if ((exp & 1) == 1) pow *= bas;
                exp >>= 1;
                if (exp == 0) break;
                bas *= bas;
            }

            return pow;
        }

        // private void Format(int arg)
        // {
        //     var formatString = _operationStack.Pop().AsString();
        //     var valueToFormat = _operationStack.Pop().GetRawValue();
        //
        //     var formatted = ValueFormatter.Format(valueToFormat, formatString);
        //
        //     _operationStack.Push(ValueFactory.Create(formatted));
        //     NextInstruction();
        //
        // }

        // private void ExceptionInfo(int arg)
        // {
        //     if (_currentFrame.LastException != null)
        //     {
        //         var excInfo = new ExceptionInfoContext(_currentFrame.LastException);
        //         _operationStack.Push(excInfo);
        //     }
        //     else
        //     {
        //         _operationStack.Push(ValueFactory.Create());
        //     }
        //     NextInstruction();
        // }
        //
        // private void ExceptionDescr(int arg)
        // {
        //     if (_currentFrame.LastException != null)
        //     {
        //         var excInfo = new ExceptionInfoContext(_currentFrame.LastException);
        //         _operationStack.Push(ValueFactory.Create(excInfo.MessageWithoutCodeFragment));
        //     }
        //     else
        //     {
        //         _operationStack.Push(ValueFactory.Create(""));
        //     }
        //     NextInstruction();
        // }
        //
        // private void ModuleInfo(int arg)
        // {
        //     var currentScript = this.CurrentScript;
        //     if (currentScript != null)
        //     {
        //         _operationStack.Push(currentScript);
        //     }
        //     else
        //     {
        //         _operationStack.Push(ValueFactory.Create());
        //     }
        //     NextInstruction();
        // }
    }
}