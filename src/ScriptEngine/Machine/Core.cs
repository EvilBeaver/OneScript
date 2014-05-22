using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    [Serializable]
    enum OperationCode
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

        // built-in functions
        Question,
        Bool,
        Number,
        Str,
        Date,
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
        Chr,
        ChrCode,
        EmptyStr,
        StrReplace,
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
        Pow,
        Sqrt,
        ExceptionInfo,
        ExceptionDescr
    }

    [Serializable]
    struct Command
    {
        public OperationCode Code;
        public int Argument;

        public override string ToString()
        {
            return Enum.GetName(typeof(OperationCode), Code) + ":" + Argument.ToString();
        }
    }

    [Serializable]
    enum DataType
    {
        Undefined,
        String,
        Number,
        Date,
        Boolean,
        Type,
        Object,
        NotAValidValue // default argument value
    }

    [Serializable]
    struct ConstDefinition
    {
        public DataType Type;
        public string Presentation;

        public override string ToString()
        {
            return Enum.GetName(typeof(DataType), Type) + ":" + Presentation;
        }
        
    }

    [Serializable]
    struct MethodInfo
    {
        public string Name;
        public bool IsFunction;
        public ParameterDefinition[] Params;

        public int ArgCount
        {
            get
            {
                return Params != null ? Params.Length : 0;
            }
        }

    }

    [Serializable]
    struct ParameterDefinition
    {
        public bool IsByValue;
        public bool HasDefaultValue;
        public int DefaultValueIndex;

        public const int UNDEFINED_VALUE_INDEX = -1;
    }

    struct TypeDescriptor
    {
        public int ID;
        public string Name;

        public override string ToString()
        {
            return Name;
        }

        public static TypeDescriptor FromDataType(DataType srcType)
        {
            return new TypeDescriptor()
            {
                ID = (int)srcType,
                Name = Enum.GetName(typeof(DataType), srcType)
            };
        }
    }

    [Serializable]
    struct SymbolBinding
    {
        public int CodeIndex;
        public int ContextIndex;
    }
}
