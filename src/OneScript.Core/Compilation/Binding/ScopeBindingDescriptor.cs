/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;

namespace OneScript.Compilation.Binding
{
    public enum ScopeBindingKind
    {
        Static,
        ThisScope,
        FrameScope
    }

    public readonly struct ScopeBindingDescriptor
    {
        private ScopeBindingDescriptor(ScopeBindingKind kind, IAttachableContext target, int scopeIndex)
        {
            if (kind == ScopeBindingKind.FrameScope && scopeIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(scopeIndex));

            Kind = kind;
            Target = target;
            ScopeIndex = scopeIndex;
        }

        public ScopeBindingKind Kind { get; }

        public IAttachableContext Target { get; }

        /// <summary>
        /// Индекс области видимости в списке ExecutionFrame.Scopes. Используется для ScopeBindingKind.FrameScope.
        /// </summary>
        public int ScopeIndex { get; }

        public static ScopeBindingDescriptor Static(IAttachableContext target)
            => new ScopeBindingDescriptor(ScopeBindingKind.Static, target, -1);

        public static ScopeBindingDescriptor ThisScope()
            => new ScopeBindingDescriptor(ScopeBindingKind.ThisScope, null, -1);

        public static ScopeBindingDescriptor FrameScope(int index)
            => new ScopeBindingDescriptor(ScopeBindingKind.FrameScope, null, index);
    }
}

