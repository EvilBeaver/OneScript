/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OneScript.Core;
using ScriptEngine.Types;

namespace ScriptEngine.Machine
{
    [Serializable]
    public enum OperationCode
    {
        Nop,
        PushVar,
        PushConst,
        PushLoc,
        PushRef,
        LoadVar,
        LoadLoc,
        AssignRef,
        Add,
        Sub,
        Mul,
        Div,
        Mod,
        Neg,
        Equals,
        Less,
        Greater,
        LessOrEqual,
        GreaterOrEqual,
        NotEqual,
        Not,
        And,
        Or,
        CallFunc,
        CallProc,
        ArgNum,
        PushDefaultArg,
        ResolveProp,
        ResolveMethodProc,
        ResolveMethodFunc,
        Jmp,
        JmpFalse,
        PushIndexed,
        Return,
        JmpCounter,
        Inc,
        NewInstance,
        PushIterator,
        IteratorNext,
        StopIterator,
        BeginTry,
        EndTry,
        RaiseException,
        LineNum,
        MakeRawValue,
        MakeBool,
        PushTmp,
        PopTmp,
        Execute,
        AddHandler,
        RemoveHandler,
        ExitTry,

        // built-in functions
        Eval,
        Bool,
        Number,
        Str,
        Date,
        Type,
        ValType,
        StrLen,
        TrimL,
        TrimR,
        TrimLR,
        Left,
        Right,
        Mid,
        StrPos,
        UCase,
        LCase,
        TCase,
        Chr,
        ChrCode,
        EmptyStr,
        StrReplace,
        StrGetLine,
        StrLineCount,
        StrEntryCount,
        Year,
        Month,
        Day,
        Hour,
        Minute,
        Second,
        BegOfYear,
        BegOfMonth,
        BegOfDay,
        BegOfHour,
        BegOfMinute,
        BegOfQuarter,
        EndOfYear,
        EndOfMonth,
        EndOfDay,
        EndOfHour,
        EndOfMinute,
        EndOfQuarter,
        WeekOfYear,
        DayOfYear,
        DayOfWeek,
        AddMonth,
        CurrentDate,
        Integer,
        Round,
        Log,
        Log10,
        Sin,
        Cos,
        Tan,
        ASin,
        ACos,
        ATan,
        Exp,
        Pow,
        Sqrt,
        Min,
        Max,
        Format,
        ExceptionInfo,
        ExceptionDescr,
        ModuleInfo
    }

    [Serializable]
    public struct Command
    {
        public OperationCode Code;
        public int Argument;

        public override string ToString()
        {
            return Enum.GetName(typeof(OperationCode), Code) + ":" + Argument.ToString();
        }
    }

    [Serializable]
    public enum DataType
    {
        Undefined,
        String,
        Number,
        Date,
        Boolean,
        Type,
        Object,
        NotAValidValue, // default argument value
        GenericValue
    }

    [Serializable]
    public struct ConstDefinition : IEquatable<ConstDefinition>
    {
        public DataType Type;
        public string Presentation;

        public override string ToString()
        {
            return Enum.GetName(typeof(DataType), Type) + ":" + Presentation;
        }

        public bool Equals(ConstDefinition other)
        {
            return Type == other.Type && string.Equals(Presentation, other.Presentation, StringComparison.Ordinal);
        }
        
    }

    [Serializable]
    public struct MethodInfo
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

    [Serializable]
    public struct AnnotationDefinition
    {
        public string Name;
        public AnnotationParameter[] Parameters;

        public int ParamCount => Parameters?.Length ?? 0;
    }

    [Serializable]
    public struct AnnotationParameter
    {
        public string Name;
        public int ValueIndex;

        [NonSerialized]
        public IValue RuntimeValue;
        
        public const int UNDEFINED_VALUE_INDEX = -1;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return string.Format("[{0}]", ValueIndex);
            }
            if (ValueIndex == UNDEFINED_VALUE_INDEX)
            {
                return Name;
            }
            return String.Format("{0}=[{1}]", Name, ValueIndex);
        }
    }

    [Serializable]
    public struct SymbolBinding
    {
        public int CodeIndex;
        public int ContextIndex;
    }

    public enum SymbolType
    {
        Variable,
        ContextProperty
    }

    [Serializable]
    public struct VariableInfo
    {
        public int Index;
        public string Identifier;
        public string Alias;
        public SymbolType Type;
        
        public bool CanGet;
        public bool CanSet;
        
        public AnnotationDefinition[] Annotations;

        public int AnnotationsCount => Annotations?.Length ?? 0;

        public override string ToString()
        {
            return $"{Index}:{Identifier}";
        }
    }

    public struct VariableBinding
    {
        public SymbolType type;
        public SymbolBinding binding;
    }
}
