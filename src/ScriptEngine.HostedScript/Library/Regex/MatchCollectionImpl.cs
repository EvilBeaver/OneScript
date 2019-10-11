﻿/*----------------------------------------------------------
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

        public MatchCollection(RegExp.MatchCollection matches)
        {
            _matches = matches;
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
                yield return new MatchImpl(item);
            }
        }
        public override IValue GetIndexedValue(IValue index)
        {
            return new MatchImpl(_matches[(int)index.AsNumber()]);
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

        public GroupCollection(RegExp.GroupCollection groups)
        {
            _groups = groups;
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
            foreach (RegExp.Group item in _groups)
            {
                yield return new GroupImpl(item);
            }
        }
        public override IValue GetIndexedValue(IValue index)
        {
            return new GroupImpl(_groups[(int)index.AsNumber()]);
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
