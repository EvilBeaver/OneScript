/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Операции со строками")]
    public class StringOperations : GlobalContextBase<StringOperations>
    {
        readonly int STRTEMPLATE_ID;
        const string STRTEMPLATE_NAME_RU = "СтрШаблон";
        const string STRTEMPLATE_NAME_EN = "StrTemplate";

        public StringOperations()
        {
            STRTEMPLATE_ID = this.Methods.Count;
        }

        /// <summary>
        /// Функция НСтр имеет ограниченную поддержку и может использоваться только для упрощения портирования кода из 1С.
        /// Возвращает только строку на первом языке из списка, если второй параметр не указан. (Игнорирует "язык по-умолчанию")
        /// </summary>
        /// <param name="src">Строка на нескольких языках</param>
        /// <param name="lang">Код языка (если не указан, возвращается первый возможный вариант)</param>
        [ContextMethod("НСтр", "NStr")]
        public string NStr(string src, string lang = null)
        {
            var parser = new FormatParametersList(src);
            string str;
            if (lang == null)
                str = parser.EnumerateValues().FirstOrDefault();
            else
                str = parser.GetParamValue(lang);

            return str == null ? String.Empty : str;
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

        [ContextMethod("StrStartWith", IsDeprecated = true, ThrowOnUse = false)]
        [Obsolete]
        public bool StrStartWith(string inputString, string searchString)
        {
            return StrStartsWith(inputString, searchString);
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
                if(!string.IsNullOrEmpty(stringDelimiter))
                {
                    arrParsed = inputString.Split(new string[] { stringDelimiter }, (bool) includeEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    arrParsed = new string[] { inputString };
                }
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

        #region IRuntimeContextInstance overrides

        public override int FindMethod(string name)
        {
            if (string.Compare(name, STRTEMPLATE_NAME_RU, true) == 0 || string.Compare(name, STRTEMPLATE_NAME_EN, true) == 0)
                return STRTEMPLATE_ID;
            else
                return base.FindMethod(name);
        }

        public override int GetMethodsCount()
        {
            return base.GetMethodsCount() + 1;
        }

        private static MethodInfo CreateStrTemplateMethodInfo()
        {
            var strTemplateMethodInfo = new MethodInfo();
            strTemplateMethodInfo.IsFunction = true;
            strTemplateMethodInfo.Name = STRTEMPLATE_NAME_RU;
            strTemplateMethodInfo.Alias = STRTEMPLATE_NAME_EN;
            strTemplateMethodInfo.Params = new ParameterDefinition[11];
            strTemplateMethodInfo.IsExport = true;

            strTemplateMethodInfo.Params[0] = new ParameterDefinition()
            {
                IsByValue = true
            };

            for (int i = 1; i < strTemplateMethodInfo.Params.Length; i++)
            {
                strTemplateMethodInfo.Params[i] = new ParameterDefinition()
                {
                    IsByValue = true,
                    HasDefaultValue = true
                };
            }
            return strTemplateMethodInfo;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            if (methodNumber == STRTEMPLATE_ID)
                return CreateStrTemplateMethodInfo();
            else
                return base.GetMethodInfo(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            if (methodNumber == STRTEMPLATE_ID)
                CallStrTemplate(arguments);
            else
                base.CallAsProcedure(methodNumber, arguments);
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            if (methodNumber == STRTEMPLATE_ID)
                retValue = CallStrTemplate(arguments);
            else
                base.CallAsFunction(methodNumber, arguments, out retValue);
        } 

        #endregion

        private IValue CallStrTemplate(IValue[] arguments)
        {
            var srcFormat = arguments[0].AsString();
            if (srcFormat == String.Empty)
                return ValueFactory.Create("");

            var re = new System.Text.RegularExpressions.Regex(@"(%%)|(%\d+)|(%\D)");
            int matchCount = 0;
            int passedArgsCount = arguments.Skip(1).Count(x => x.DataType != DataType.NotAValidValue && x.DataType != DataType.Undefined);
            var result = re.Replace(srcFormat, (m) =>
            {
                if (m.Groups[1].Success)
                    return "%";
                
                if(m.Groups[2].Success)
                {
                    matchCount++;
                    var number = int.Parse(m.Groups[2].Value.Substring(1));
                    if (number < 1 || number > 11)
                        throw new RuntimeException("Ошибка при вызове метода контекста (СтрШаблон): Ошибка синтаксиса шаблона в позиции " + (m.Index + 1));

                    if (arguments[number] != null && arguments[number].DataType != DataType.NotAValidValue)
                        return arguments[number].AsString();
                    else
                        return "";
                }

                throw new RuntimeException("Ошибка при вызове метода контекста (СтрШаблон): Ошибка синтаксиса шаблона в позиции " + (m.Index + 1));

            });

            if (passedArgsCount > matchCount)
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
