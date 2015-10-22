
/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine;
using RegExp = System.Text.RegularExpressions;


namespace ScriptEngine.HostedScript.Library.RegexLib
{
    [ContextClass("СовпадениеРегулярногоВыражения", "RegExMatch")]
    class MatchImpl : AutoContext<MatchImpl>
    {
        private RegExp.Match _match;

        public MatchImpl(RegExp.Match match)
        {
            _match = match;
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
            get { return new GroupCollection(_match.Groups); }
        }
    }

    [ContextClass("ГруппаРегулярногоВыражения", "RegExGroup")]
    class GroupImpl : AutoContext<GroupImpl>
    {
        private RegExp.Group _group;

        public GroupImpl(RegExp.Group group)
        {
            _group = group;
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
    }
}
