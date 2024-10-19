/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Оборачивающий контейнер. Хранит внешние области видимости и одну локальную, как единый плоский список.
    /// </summary>
    public class RuntimeScopes : IReadOnlyList<AttachedContext>
    {
        private readonly IReadOnlyList<AttachedContext> _outerScopes;
        private readonly AttachedContext _innerScope;

        private readonly int _outerScopeLast;

        public RuntimeScopes(IReadOnlyList<AttachedContext> outerScopes, AttachedContext innerScope)
        {
            _outerScopes = outerScopes.Select(x => new AttachedContext(x.Instance)).ToList();
            _innerScope = innerScope;

            _outerScopeLast = _outerScopes.Count - 1;
        }

        public IEnumerator<AttachedContext> GetEnumerator()
        {
            foreach (var scope in _outerScopes)
            {
                yield return scope;
            }

            yield return _innerScope;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _outerScopes.Count + 1;

        public AttachedContext this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                if (index > _outerScopeLast)
                    return _innerScope;

                return _outerScopes[index];
            }
        }
    }
}