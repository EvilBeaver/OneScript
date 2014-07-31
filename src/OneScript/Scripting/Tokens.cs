using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public enum Token
    {
        NotAToken,

        // structure
        VarDef,
        Procedure,
        EndProcedure,
        Function,
        EndFunction,
        If,
        Then,
        Else,
        ElseIf,
        EndIf,
        For,
        While,
        Each,
        To,
        In,
        Loop,
        EndLoop,
        Break,
        Continue,
        Return,
        Try,
        Exception,
        RaiseException,
        EndTry,
        EndOfText,
        Export,

        // operators
        Plus,
        Minus,
        UnaryMinus,
        Multiply,
        Division,
        Modulo,
        Equal,
        MoreThan,
        LessThan,
        MoreOrEqual,
        LessOrEqual,
        NotEqual,
        And,
        Or,
        Not,
        Dot,
        OpenPar,
        ClosePar,
        OpenBracket,
        CloseBracket,
        NewObject,

        // special chars
        Comma,
        StringQuote,
        DateQuote,
        Semicolon,

        // modifiers
        ByValParam,

        // built-in functions
        // must be declared last
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
}
