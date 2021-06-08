/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;

namespace ScriptEngine.Machine
{
    [Serializable]
    public struct MethodSignature
    {
        public string Name;
        public string Alias;
        public ParameterDefinition[] Params;
        public AnnotationDefinition[] Annotations;
        public MethodFlags Flags;

        public bool IsFunction
        {
            get => IsSet(MethodFlags.IsFunction);
            set => SetFlag(MethodFlags.IsFunction, value);
        }
        
        public bool IsExport
        {
            get => IsSet(MethodFlags.IsExported);
            set => SetFlag(MethodFlags.IsExported, value);
        }
        
        public bool IsAsync
        {
            get => IsSet(MethodFlags.IsAsync);
            set => SetFlag(MethodFlags.IsAsync, value);
        }
        
        public bool IsDeprecated
         {
            get => IsSet(MethodFlags.IsDeprecated);
            set => SetFlag(MethodFlags.IsDeprecated, value);
        }
        
        public bool ThrowOnUseDeprecated
        {
            get => IsSet(MethodFlags.ThrowOnUsage);
            set => SetFlag(MethodFlags.ThrowOnUsage, value);
        }

        public int ArgCount => Params?.Length ?? 0;
        public int AnnotationsCount => Annotations?.Length ?? 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFlag(MethodFlags flag) => Flags |= flag;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFlag(MethodFlags flag, bool value)
        { 
            if(value) SetFlag(flag);
            else ClearFlag(flag);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearFlag(MethodFlags flag) => Flags &= ~flag;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSet(MethodFlags flag) => (Flags & flag) != 0;
    }

    [Serializable]
    [Flags]
    public enum MethodFlags
    {
        Default = 0,
        IsFunction = 1,
        IsExported = 2,
        IsDeprecated = 4,
        ThrowOnUsage = 8,
        IsAsync = 16
    }
    
    [Serializable]
    public struct ParameterDefinition
    {
        public string Name;
        public bool IsByValue;
        public bool HasDefaultValue;
        public int DefaultValueIndex;
        public AnnotationDefinition[] Annotations;

        public int AnnotationsCount => Annotations?.Length ?? 0;

        public const int UNDEFINED_VALUE_INDEX = -1;

        public bool IsDefaultValueDefined()
        {
            return HasDefaultValue && DefaultValueIndex != UNDEFINED_VALUE_INDEX;
        }
    }
}