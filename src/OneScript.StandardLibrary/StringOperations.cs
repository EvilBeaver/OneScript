/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    [GlobalContext(Category = "Операции со строками")]
    public class StringOperations : GlobalContextBase<StringOperations>
    {
        /// <summary>
        /// Получает строку на языке, заданном во втором параметре (коды языков в соответствии с  ISO 639-1)
        /// или на текущем языке системы.
        /// </summary>
        /// <param name="src">Строка на нескольких языках</param>
        /// <param name="lang">Код языка (если не указан, возвращает вариант для текущего языка системы, 
        /// если вариант не найден, то возвращает вариант для английского языка, если не задан вариант для английского языка,
        /// то возвращает первый вариант из списка)</param>
        [ContextMethod("НСтр", "NStr")]
        public string NStr(string src, string lang = null)
        {
            return Locale.NStr(src, lang);
        }

        /// <summary>
        /// Определяет, что строка начинается с указанной подстроки.
        /// </summary>
        /// <param name="inputString">Строка, начало которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое начало строки. В случае если переданное значение является пустой строкой генерируется исключительная ситуация.</param>
        [ContextMethod("СтрНачинаетсяС", "StrStartsWith")]
        public bool StrStartsWith(string inputString, string searchString)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.StartsWith(searchString);
                }
                else throw new RuntimeException("Ошибка при вызове метода контекста (СтрНачинаетсяС): Недопустимое значение параметра (параметр номер '2')"); 
            }

            return result;
        }

        /// <summary>
        /// Определяет, заканчивается ли строка указанной подстрокой.
        /// </summary>
        /// <param name="inputString">Строка, окончание которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое окончание строки. В случае если переданное значение является пустой строкой генерируется исключительная ситуация.</param>
        [ContextMethod("СтрЗаканчиваетсяНа", "StrEndsWith")]
        public bool StrEndsWith(string inputString, string searchString)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.EndsWith(searchString);
                }
                else throw new RuntimeException("Ошибка при вызове метода контекста (СтрЗаканчиваетсяНа): Недопустимое значение параметра (параметр номер '2')"); 
            }

            return result;
        }

        /// <summary>
        /// Разделяет строку на части по указанным символам-разделителям.
        /// </summary>
        /// <param name="inputString">Разделяемая строка.</param>
        /// <param name="stringDelimiter">Строка символов, каждый из которых является индивидуальным разделителем.</param>
        /// <param name="includeEmpty">Указывает необходимость включать в результат пустые строки, которые могут образоваться в результате разделения исходной строки. Значение по умолчанию: Истина. </param>
        [ContextMethod("СтрРазделить", "StrSplit")]
        public ArrayImpl StrSplit(string inputString, string stringDelimiter, bool? includeEmpty = true)
        {
            string[] arrParsed;
            if (includeEmpty == null)
                includeEmpty = true;
            
            if(!string.IsNullOrEmpty(inputString))
            {
                arrParsed = inputString.Split(stringDelimiter?.ToCharArray(), (bool) includeEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                arrParsed = (bool) includeEmpty ? new string[] { string.Empty } : new string[0];
            }
            return new ArrayImpl(arrParsed.Select(x => ValueFactory.Create(x)));
        }

        /// <summary>
        /// Соединяет массив переданных строк в одну строку с указанным разделителем
        /// </summary>
        /// <param name="input">Массив - соединяемые строки</param>
        /// <param name="delimiter">Разделитель. Если не указан, строки объединяются слитно</param>
        [ContextMethod("СтрСоединить", "StrConcat")]
        public string StrConcat(ArrayImpl input, string delimiter = null)
        {
            var strings = input.Select(x => x.AsString());
            
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
        public int StrFind(string haystack, string needle, SearchDirection direction = SearchDirection.FromBegin, int startPos = 0, int occurance = 0)
        {
            int len = haystack.Length;
            if (len == 0 || needle.Length == 0)
                return 0;

            bool fromBegin = direction == SearchDirection.FromBegin;

            if(startPos == 0)
            {
                startPos = fromBegin ? 1 : len;
            }

            if (startPos < 1 || startPos > len)
                throw RuntimeException.InvalidArgumentValue();

            if (occurance == 0)
                occurance = 1;

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

            if (foundTimes == occurance)
                return index + 1;
            else
                return 0;
        }

        /// <summary>
        /// Подставляет параметры в строку по номеру
        /// </summary>
        /// <param name="template">Шаблон: строка, содержащая маркеры подстановки вида %N</param>
        /// <param name="p1-p10">Параметры, строковые представления которых должны быть подставлены в шаблон</param>
        /// <returns>Строка шаблона с подставленными параметрами</returns>
        [ContextMethod("СтрШаблон", "StrTemplate")]
        public IValue StrTemplate(IValue template,
            IValue p1=null, IValue p2=null, IValue p3=null, IValue p4=null, IValue p5=null,
            IValue p6=null, IValue p7=null, IValue p8=null, IValue p9=null, IValue p10=null)
        {
            var srcFormat = template?.AsString() ?? "";

            var arguments = new IValue[] { p10,p9,p8,p7,p6,p5,p4,p3,p2,p1 };
            int passedArgsCount = arguments
                .SkipWhile(x => x == null || x.IsSkippedArgument() || x.SystemType == BasicTypes.Undefined)
                .Count();

            var re = new System.Text.RegularExpressions.Regex(@"(%%)|%(\d+)|%\((\d+)\)|%");
            int maxNumber = 0;
            var result = re.Replace(srcFormat, (m) =>
            {
                if (m.Groups[1].Success)
                    return "%";
                
                if(m.Groups[2].Success || m.Groups[3].Success)
                {
                    var number = int.Parse(m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Value);

                    if (number < 1 || number > 10)
                        throw new RuntimeException($"Ошибка синтаксиса шаблона в позиции {m.Index+2}: недопустимый номер подстановки");

                    //FIXME: отключено, т.к. платформа игнорирует ошибку с недостаточным числом параметров
                    //if (number > passedArgsCount)
                    //    throw RuntimeException.TooFewArgumentsPassed();
                    
                    if (number > maxNumber)
                        maxNumber = number;

                    var arg = arguments[10-number];
                    if ( arg!= null && !arg.IsSkippedArgument())
                        return arg.AsString();
                    else
                        return "";
                }

                throw new RuntimeException("Ошибка синтаксиса шаблона в позиции " + (m.Index + 2));
            });

            if (passedArgsCount > maxNumber)
                throw RuntimeException.TooManyArgumentsPassed();

            return ValueFactory.Create(result);
        }

        public static IAttachableContext CreateInstance()
        {
            return new StringOperations();
        }

    }

    [EnumerationType("НаправлениеПоиска", "SearchDirection")]
    public enum SearchDirection
    {
        [EnumItem("СНачала")]
        FromBegin,
        [EnumItem("СКонца")]
        FromEnd
    }

    
}
