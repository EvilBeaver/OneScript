/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Contexts.Enums;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Localization;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Linq;

namespace OneScript.StandardLibrary
{
    [GlobalContext(Category = "Операции со строками")]
    public class StringOperations : GlobalContextBase<StringOperations>
    {
        private static readonly System.Text.RegularExpressions.Regex _templateRe
            = new System.Text.RegularExpressions.Regex(@"(%%)|%(\d+)|%\((\d+)\)|%",
                  System.Text.RegularExpressions.RegexOptions.Compiled);

        /// <summary>
        /// Получает строку на языке, заданном во втором параметре (коды языков в соответствии с  ISO 639-1)
        /// или на текущем языке системы.
        /// </summary>
        /// <param name="src">Строка на нескольких языках</param>
        /// <param name="lang">Код языка (если не указан, возвращает вариант для текущего языка системы, 
        /// если вариант не найден, то возвращает вариант для английского языка,
        /// если не задан вариант для английского языка, то возвращает первый вариант из списка)</param>
        [ContextMethod("НСтр", "NStr")]
        public string NStr(string src, string lang = null)
        {
            return Locale.NStr(src, lang);
        }

        /// <summary>
        /// Определяет, что строка начинается с указанной подстроки.
        /// </summary>
        /// <param name="inputString">Строка, начало которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое начало строки. 
        /// В случае, если переданное значение является пустой строкой, генерируется исключительная ситуация.</param>
        [ContextMethod("СтрНачинаетсяС", "StrStartsWith")]
        public bool StrStartsWith(string inputString, string searchString)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.StartsWith(searchString);
                }
                else throw StringOpException.StrStartsWith();
            }

            return result;
        }

        /// <summary>
        /// Определяет, заканчивается ли строка указанной подстрокой.
        /// </summary>
        /// <param name="inputString">Строка, окончание которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое окончание строки.
        /// В случае, если переданное значение является пустой строкой, генерируется исключительная ситуация.</param>
        [ContextMethod("СтрЗаканчиваетсяНа", "StrEndsWith")]
        public bool StrEndsWith(string inputString, string searchString)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.EndsWith(searchString);
                }
                else throw StringOpException.StrEndsWith();
            }

            return result;
        }

        /// <summary>
        /// Разделяет строку на части по указанным символам-разделителям.
        /// </summary>
        /// <param name="inputString">Разделяемая строка.</param>
        /// <param name="stringDelimiter">Строка символов, каждый из которых является индивидуальным разделителем.</param>
        /// <param name="includeEmpty">Указывает необходимость включать в результат пустые строки, 
        /// которые могут образоваться в результате разделения исходной строки. Значение по умолчанию: Истина. </param>
        [ContextMethod("СтрРазделить", "StrSplit")]
        public ArrayImpl StrSplit(string inputString, string stringDelimiter, bool? includeEmpty = true)
        {
            string[] arrParsed;
            if (includeEmpty == null)
                includeEmpty = true;

            if (!string.IsNullOrEmpty(inputString))
            {
                arrParsed = inputString.Split(stringDelimiter?.ToCharArray(),
                                              (bool)includeEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                arrParsed = (bool)includeEmpty ? new string[] { string.Empty } : Array.Empty<string>();
            }

            return new ArrayImpl(arrParsed.Select(x => ValueFactory.Create(x)));
        }

        /// <summary>
        /// Соединяет массив переданных строк в одну строку с указанным разделителем
        /// </summary>
        /// <param name="input">Массив - соединяемые строки</param>
        /// <param name="delimiter">Разделитель. Если не указан, строки объединяются слитно</param>
        [ContextMethod("СтрСоединить", "StrConcat")]
        public string StrConcat(IBslProcess process, ArrayImpl input, string delimiter = null)
        {
            var strings = input.Select(x => x.AsString(process));

            return String.Join(delimiter, strings);
        }

        /// <summary>
        /// Сравнивает строки без учета регистра.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>-1 первая строка больше, 1 - вторая строка больше. 0 - строки равны</returns>
        [ContextMethod("СтрСравнить", "StrCompare")]
        public int StrCompare(string first, string second)
        {
            return String.Compare(first, second, true);
        }

        /// <summary>
        /// Находит вхождение искомой строки как подстроки в исходной строке
        /// </summary>
        /// <param name="haystack">Строка, в которой ищем</param>
        /// <param name="needle">Строка, которую надо найти</param>
        /// <param name="direction">значение перечисления НаправлениеПоиска (с конца/с начала)</param>
        /// <param name="startPos">Начальная позиция, с которой начинать поиск</param>
        /// <param name="occurance">Указывает номер вхождения искомой подстроки в исходной строке</param>
        /// <returns>Позицию искомой строки в исходной строке. Возвращает 0, если подстрока не найдена.</returns>
        [ContextMethod("СтрНайти", "StrFind")]
        public int StrFind(string haystack, string needle, SearchDirection direction = SearchDirection.FromBegin, int startPos = 0, int occurance = 1)
        {
            if (needle == null || needle.Length == 0)
                return 1;

            int len = haystack?.Length ?? 0;
            if (len == 0)
                return 0;

            bool fromBegin = direction == SearchDirection.FromBegin;

            if (startPos == 0)
            {
                startPos = fromBegin ? 1 : len;
            }
            else if (startPos < 1 || startPos > len)
                throw RuntimeException.InvalidNthArgumentValue(4);

            if (occurance < 0)
                return 0;
            else if (occurance == 0)
                throw RuntimeException.InvalidNthArgumentValue(5);

            int startIndex = startPos - 1;
            int foundTimes = 0;
            int index = len + 1;

            if(fromBegin)
            {
                while(foundTimes < occurance && index >= 0)
                {
                    index = haystack.IndexOf(needle, startIndex, StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        startIndex = index + 1;
                        foundTimes++;
                    }
                    if (startIndex >= len)
                        break;
                }
            }
            else
            {
                while(foundTimes < occurance && index >= 0)
                {
                    index = haystack.LastIndexOf(needle, startIndex, StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        startIndex = index - 1;
                        foundTimes++;
                    }
                    if (startIndex < 0)
                        break;
                }
            }

            return foundTimes == occurance ? index + 1 : 0;
        }

        /// <summary>
        /// Подставляет параметры в строку по номеру
        /// </summary>
        /// <param name="template">Шаблон: строка, содержащая маркеры подстановки вида %N</param>
        /// <param name="p1-p10">Параметры, строковые представления которых должны быть подставлены в шаблон</param>
        /// <returns>Строка шаблона с подставленными параметрами</returns>
        [ContextMethod("СтрШаблон", "StrTemplate")]
        public string StrTemplate(string template,
            string p1=null, string p2=null, string p3=null, string p4=null, string p5=null,
            string p6=null, string p7=null, string p8=null, string p9=null, string p10=null)
        {
            var srcFormat = template ?? "";

            var arguments = new [] { p10,p9,p8,p7,p6,p5,p4,p3,p2,p1 };
            int passedArgsCount = arguments
                .SkipWhile(x => x == null)
                .Count();

            int maxNumber = 0;
            var result = _templateRe.Replace(srcFormat, (m) =>
            {
                if (m.Groups[1].Success)
                    return "%";
                
                if(m.Groups[2].Success || m.Groups[3].Success)
                {
                    var number = int.Parse(m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value);

                    if (number < 1 || number > 10)
                        throw StringOpException.TemplateSubst(m.Index + 2, number);

                    //FIXME: отключено, т.к. платформа игнорирует ошибку с недостаточным числом параметров
                    //if (number > passedArgsCount)
                    //    throw RuntimeException.TooFewArgumentsPassed();
                    
                    if (number > maxNumber)
                        maxNumber = number;

                    return arguments[10-number] ?? "";
                }

                throw StringOpException.TemplateSyntax(m.Index + 2);
            });

            if (passedArgsCount > maxNumber)
                throw RuntimeException.TooManyArgumentsPassed();

            return result;
        }

        public static IAttachableContext CreateInstance()
        {
            return new StringOperations();
        }
    }


    [EnumerationType("НаправлениеПоиска", "SearchDirection")]
    public enum SearchDirection
    {
        [EnumValue("СНачала", "FromBegin")]
        FromBegin,
        [EnumValue("СКонца", "FromEnd")]
        FromEnd
    }

    
    public class StringOpException : RuntimeException
    {
        public StringOpException(BilingualString message, Exception innerException) : base(message,
            innerException)
        {}

        public StringOpException(BilingualString message) : base(message)
        {}

        public static StringOpException StrStartsWith()
        {
            return new StringOpException(new BilingualString(
                "Ошибка при вызове метода контекста (СтрНачинаетсяС): Недопустимое значение параметра номер '2'",
                "Error calling context method (StrStartsWith): Invalid parameter number '2' value"));
        }
        public static StringOpException StrEndsWith()
        {
            return new StringOpException(new BilingualString(
                "Ошибка при вызове метода контекста (СтрЗаканчиваетсяНа): Недопустимое значение параметра номер '2'",
                "Error calling context method (StrEndsWith): Invalid parameter number '2' value"));
        }

        public static StringOpException TemplateSyntax(int pos)
        {
            return new StringOpException(new BilingualString(
                $"Ошибка синтаксиса шаблона в позиции {pos}",
                $"Template syntax error at position {pos}"));
        }

        public static StringOpException TemplateSubst(int pos, int num)
        {
            return new StringOpException(new BilingualString(
                $"Ошибка синтаксиса шаблона в позиции {pos}. Недопустимый номер подстановки: '{num}'",
                $"Template syntax error at position {pos}. Invalid substitution number: '{num}'"));
        }
     }

}
