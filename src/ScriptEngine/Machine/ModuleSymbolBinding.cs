/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Compilation.Binding;

namespace ScriptEngine.Machine
{
    public struct ModuleSymbolBinding : IEquatable<ModuleSymbolBinding>
    {
        public IAttachableContext Target { get; set; }

        public int MemberNumber { get; set; }

        public ScopeBindingKind Kind { get; set; }

        public int ScopeIndex { get; set; }

        internal IAttachableContext ResolveTarget(ExecutionFrame frame)
        {
            return Kind switch
            {
                ScopeBindingKind.FrameScope => ResolveFrameScope(frame),
                ScopeBindingKind.ThisScope => frame.ThisScope,
                _ => Target
            };
        }

        private IAttachableContext ResolveFrameScope(ExecutionFrame frame)
        {
            if (frame?.Scopes == null)
                throw new InvalidOperationException("Frame scopes are not available");
            if (ScopeIndex < 0 || ScopeIndex >= frame.Scopes.Count)
                throw new InvalidOperationException($"Invalid scope index {ScopeIndex}");

            return frame.Scopes[ScopeIndex];
        }

        public bool Equals(ModuleSymbolBinding other)
        {
            return ReferenceEquals(Target, other.Target)
                   && MemberNumber == other.MemberNumber
                   && Kind == other.Kind
                   && ScopeIndex == other.ScopeIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleSymbolBinding other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Target, MemberNumber, Kind, ScopeIndex);
        }
    }
}