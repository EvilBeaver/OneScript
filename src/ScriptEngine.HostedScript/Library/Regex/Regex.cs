/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

using RegExp = System.Text.RegularExpressions;

namespace ScriptEngine.HostedScript.Library.Regex
{
    [ContextClass("РегулярноеВыражение", "Regex")]
    public class RegExpImpl : AutoContext<RegExpImpl>
    {
        private RegExp.Regex _regex;
        private readonly string _pattern;

        public RegExpImpl(string pattern)
        {
            _pattern = pattern;
            _regex = new RegExp.Regex(_pattern, RegExp.RegexOptions.IgnoreCase | RegExp.RegexOptions.Multiline );
        }

        /// <summary>
        /// Проверяет, что в строке есть совпадение с шаблоном регулярного выражения.
        /// </summary>
        /// <param name="inputString">Строка, которая проверяется.</param>
        /// <param name="startAt">Число. Необязательный параметр. По умолчанию 0. Содержит стартовую позицию, начиная с которой надо анализировать текст.
        /// Нумерация позиций, в отличие от 1С, начинается с 0</param>
        /// <returns>Признак совпадения. Булево.</returns>
        [ContextMethod("Совпадает", "IsMatch")]
        public IValue IsMatch(string inputString, int startAt = 0)
        {
            return ValueFactory.Create(_regex.IsMatch(inputString, startAt));
        }

        /// <summary>
        /// Находит все совпадения в строке по шаблону регулярного выражения.
        /// </summary>
        /// <param name="inputString">Строка, которая проверяется.</param>
        /// <param name="startAt">Число. Необязательный параметр. По умолчанию 0. Содержит стартовую позицию, начиная с которой надо анализировать текст.
        /// Нумерация позиций, в отличие от 1С, начинается с 0</param>
        /// <returns>Коллекция совпадения (тип КоллекцияСовпаденийРегулярногоВыражения).</returns>
        [ContextMethod("НайтиСовпадения", "Matches")]
        public MatchCollection Matches(string inputString, int startAt = 0)
        {
            return new MatchCollection(_regex.Matches(inputString, startAt), _regex);
        }

        /// <summary>
        /// Разделяет исходную строку на части, используя как разделитель заданное регулярное выражение.
        /// </summary>
        /// <param name="inputString">Строка, которая проверяется.</param>
        /// <param name="count">Число. Необязательный параметр. По умолчанию 0 (искать все). Содержит количество искомых элементов.</param>
        /// <param name="startAt">Число. Необязательный параметр. По умолчанию 0. Содержит стартовую позицию, начиная с которой надо анализировать текст.
        /// Нумерация позиций, в отличие от 1С, начинается с 0</param>
        /// <returns>Массив полученных строк.</returns>
        [ContextMethod("Разделить", "Split")]
        public ArrayImpl Split(string inputString, int count = 0, int startAt = 0)
        {
            string[] arrParsed = _regex.Split(inputString, count, startAt);
            return new ArrayImpl(arrParsed.Select(x => ValueFactory.Create(x)));
        }

        /// <summary>
        /// Заменяет в исходной строке все вхождения регулярного выражения на СтрокаЗамены.
        /// В строке замены можно использовать ссылки на захваченные группы как $n, где n - номер захваченной группы ($0 - все захваченное выражение).
        /// </summary>
        /// <param name="inputString">Строка. Текст, в котором необходимо выполнить замены.</param>
        /// <param name="replacement">Строка. Текст, который будет вставляться в места замены.</param>
        /// <returns>Строку-результат замены.</returns>
        [ContextMethod("Заменить", "Replace")]
        public string Replace(string inputString, string replacement)
        {
            return _regex.Replace(inputString, replacement);
        }

        /// <summary>
        /// Признак Не учитывать регистр символов. Булево
        /// </summary>
        [ContextProperty("ИгнорироватьРегистр", "IgnoreCase")]
        public bool IgnoreCase
        {
            get { return _regex.Options.HasFlag( RegExp.RegexOptions.IgnoreCase ); }
            set { SetOption(value, RegExp.RegexOptions.IgnoreCase); }
        }

        /// <summary>
        /// Признак выполнения многострочного поиска. Булево
        /// </summary>
        [ContextProperty("Многострочный", "Multiline")]
        public bool Multiline
        {
            get { return _regex.Options.HasFlag(RegExp.RegexOptions.Multiline); }
            set { SetOption(value, RegExp.RegexOptions.Multiline); }
        }

        /// <summary>
        /// Конструктор создания регулярного выражения по заданному шаблону.
        /// </summary>
        /// <param name="pattern">Строка-шаблон регулярного выражения.</param>
        [ScriptConstructor(Name = "По регулярному выражению")]
        public static RegExpImpl Constructor(IValue pattern)
        {
            var regex = new RegExpImpl(pattern.AsString());
            return regex;
        }

        private void SetOption(bool value, RegExp.RegexOptions option)
        {
                var options = _regex.Options;
                if (value)
                    options |= option;
                else
                    options &= ~option;

                //приходится пересоздавать объект, т.к. опции объекта по умолчанию только read-only
                _regex = new RegExp.Regex(_pattern, options);
        }
    }
}
