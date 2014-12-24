using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Runtime
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
    public struct Command
    {
        public OperationCode Code;
        public int Argument;

        public override string ToString()
        {
            return Enum.GetName(typeof(OperationCode), Code) + ":" + Argument.ToString();
        }
    }
}
