/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Contexts
{
    /// <summary>
    /// Класс контекста, который подключен в память машины.
    /// </summary>
    public sealed class AttachedContext
    {
        private IVariable[] _variables;
        private BslMethodInfo[] _methods;

        public AttachedContext(IAttachableContext source)
        {
            Instance = source;
            Attach();
        }

        public void Attach()
        {
            Instance.OnAttach(out _variables, out _methods);
        }

        public AttachedContext(IAttachableContext target, IVariable[] vars)
        {
            Instance = target;
            _variables = vars;
            _methods = Array.Empty<BslMethodInfo>();
        }

        public IReadOnlyList<IVariable> Variables => _variables;

        public IReadOnlyList<BslMethodInfo> Methods => _methods;

        public IAttachableContext Instance { get; }
    }
}