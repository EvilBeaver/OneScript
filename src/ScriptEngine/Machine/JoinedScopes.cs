/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Оборачивающий контейнер. Хранит внешние области видимости и одну локальную, как единый плоский список.
    /// </summary>
    public class JoinedScopes : IReadOnlyList<IAttachableContext>
    {
        private readonly IReadOnlyList<IAttachableContext> _outerScopes;
        private readonly IAttachableContext _innerScope;
        private readonly IReadOnlyList<IAttachableContext> _rootScopes;
        private readonly int _innerScopesCount;

        public JoinedScopes(IReadOnlyList<IAttachableContext> outerScopes, IAttachableContext innerScope)
        {
            _outerScopes = outerScopes ?? throw new ArgumentNullException(nameof(outerScopes));
            _innerScope = innerScope ?? throw new ArgumentNullException(nameof(innerScope));

            if (_outerScopes is JoinedScopes outer)
            {
                _rootScopes = outer._rootScopes;
                _innerScopesCount = outer._innerScopesCount + 1;
            }
            else
            {
                _rootScopes = _outerScopes;
                _innerScopesCount = 1;
            }
        }

        /// <summary>
        /// Исходный список, к которому добавлены внутренние области (глобальные контексты окружения).
        /// Может расти, пока выполняется код.
        /// </summary>
        public IReadOnlyList<IAttachableContext> RootScopes => _rootScopes;

        /// <summary>
        /// Число внутренних областей, добавленных к исходному списку.
        /// </summary>
        public int InnerScopesCount => _innerScopesCount;

        /// <summary>
        /// Внутренняя область по номеру с конца: 0 - самая внутренняя.
        /// В отличие от индексатора не зависит от размера исходного списка.
        /// </summary>
        public IAttachableContext FromEnd(int index)
        {
            if (index < 0 || index >= _innerScopesCount)
                throw new ArgumentOutOfRangeException(nameof(index));

            var scopes = this;
            for (; index > 0; index--)
            {
                scopes = (JoinedScopes)scopes._outerScopes;
            }

            return scopes._innerScope;
        }

        public IEnumerator<IAttachableContext> GetEnumerator()
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

        public IAttachableContext this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException();

                if (index == _outerScopes.Count)
                    return _innerScope;

                return _outerScopes[index];
            }
        }
    }
}

