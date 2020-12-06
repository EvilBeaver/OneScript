/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;
using RegExp = System.Text.RegularExpressions;

namespace OneScript.StandardLibrary.Regex
{
    [ContextClass("СовпадениеРегулярногоВыражения", "RegExMatch")]
    public class MatchImpl : AutoContext<MatchImpl>
    {
        private readonly RegExp.Match _match;
        private readonly RegExp.Regex _regex;

        public MatchImpl(RegExp.Match match, RegExp.Regex regex)
        {
            _match = match;
            _regex = regex;
        }

        /// <summary>
        /// Найденная строка.
        /// </summary>
        [ContextProperty("Значение", "Value")]
        public string Value
        {
            get { return _match.Value; }
        }

        /// <summary>
        /// Индекс найденной строки. Нумерация начинается с 0
        /// </summary>
        [ContextProperty("Индекс", "Index")]
        public int Index
        {
            get { return _match.Index; }
        }

        /// <summary>
        /// Длина найденной строки.
        /// </summary>
        [ContextProperty("Длина", "Length")]
        public int Length
        {
            get { return _match.Length; }
        }

        /// <summary>
        /// Коллекция найденных групп (тип КоллекцияГруппРегулярногоВыражения).
        /// </summary>
        [ContextProperty("Группы", "Groups")]
        public GroupCollection Groups
        {
            get { return new GroupCollection(_match.Groups, _regex); }
        }
    }

    [ContextClass("ГруппаРегулярногоВыражения", "RegExGroup")]
    public class GroupImpl : AutoContext<GroupImpl>
    {
        private readonly RegExp.Group _group;
        private readonly int _group_index;
        private readonly RegExp.Regex _regex;

        public GroupImpl(RegExp.Group group, int group_index, RegExp.Regex regex)
        {
            _group = group;
            _group_index = group_index;
            _regex = regex;
        }

        /// <summary>
        /// Найденная строка.
        /// </summary>
        [ContextProperty("Значение", "Value")]
        public string Value
        {
            get { return _group.Value; }
        }

        /// <summary>
        /// Индекс найденной строки. Нумерация начинается с 0
        /// </summary>
        [ContextProperty("Индекс", "Index")]
        public int Index
        {
            get { return _group.Index; }
        }

        /// <summary>
        /// Длина найденной строки.
        /// </summary>
        [ContextProperty("Длина", "Length")]
        public int Length
        {
            get { return _group.Length; }
        }

        /// <summary>
        /// Имя группы
        /// </summary>
        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get { return _regex.GroupNameFromNumber(_group_index); }
        }
    }
}