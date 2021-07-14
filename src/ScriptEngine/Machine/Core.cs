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
        BegOfWeek,
        BegOfYear,
        BegOfMonth,
        BegOfDay,
        BegOfHour,
        BegOfMinute,
        BegOfQuarter,
        EndOfWeek,
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
