/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

using RegExp = System.Text.RegularExpressions;

namespace ScriptEngine.HostedScript.Library.Regex
{
    [ContextClass("КоллекцияСовпаденийРегулярногоВыражения", "RegExMatchCollection")]
    public class MatchCollection : AutoContext<MatchCollection>, ICollectionContext, IEnumerable<MatchImpl>
    {
        private readonly RegExp.MatchCollection _matches;
        private readonly RegExp.Regex _regex;

        public MatchCollection(RegExp.MatchCollection matches, RegExp.Regex regex)
        {
            _matches = matches;
            _regex = regex;
        }

        #region ICollectionContext Members

        /// <summary>
        /// Получает количество полученных совпадений.
        /// </summary>
        /// <returns>Количество полученных совпадений.</returns>
        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _matches.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IRuntimeContextInstance> Members

        public IEnumerator<MatchImpl> GetEnumerator()
        {
            foreach (RegExp.Match item in _matches)
            {
                yield return new MatchImpl(item, _regex);
            }
        }
        public override IValue GetIndexedValue(IValue index)
        {
            return new MatchImpl(_matches[(int)index.AsNumber()], _regex);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }

    [ContextClass("КоллекцияГруппРегулярногоВыражения", "RegExGroupCollection")]
    public class GroupCollection : AutoContext<GroupCollection>, ICollectionContext, IEnumerable<GroupImpl>
    {
        private readonly RegExp.GroupCollection _groups;
        private readonly RegExp.Regex _regex;

        public GroupCollection(RegExp.GroupCollection groups, RegExp.Regex regex)
        {
            _groups = groups;
            _regex = regex;
        }

        #region ICollectionContext Members

        /// <summary>
        /// Получает количество полученных групп.
        /// </summary>
        /// <returns>Количество полученных групп.</returns>
        [ContextMethod("Количество", "Count")]
        public int Count()
        {
            return _groups.Count;
        }

        public CollectionEnumerator GetManagedIterator()
        {
            return new CollectionEnumerator(GetEnumerator());
        }

        #endregion

        #region IEnumerable<IRuntimeContextInstance> Members

        public IEnumerator<GroupImpl> GetEnumerator()
        {
            int i = 0;
            foreach (RegExp.Group item in _groups)
            {
                yield return new GroupImpl(item, i, _regex);
                i++;
            }
        }
        public override IValue GetIndexedValue(IValue index)
        {
            return new GroupImpl(_groups[(int)index.AsNumber()], (int)index.AsNumber(), _regex);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
