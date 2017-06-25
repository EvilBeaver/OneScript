/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
        LineNum,
        MakeRawValue,
        MakeBool,
        PushTmp,
        PopTmp,

        // built-in functions
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
        StrTemplate,
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
        Enumeration,
        GenericValue
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
    public struct MethodInfo
    {
        public string Name;
        public string Alias;
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
    public struct ParameterDefinition
    {
        public bool IsByValue;
        public bool HasDefaultValue;
        public int DefaultValueIndex;

        public const int UNDEFINED_VALUE_INDEX = -1;
    }

    public struct TypeDescriptor : IEquatable<TypeDescriptor>
    {
        public int ID;
        public string Name;

        public override string ToString()
        {
            return Name;
        }

        public static TypeDescriptor FromDataType(DataType srcType)
        {
            System.Diagnostics.Debug.Assert(
                   srcType == DataType.Boolean
                || srcType == DataType.Date
                || srcType == DataType.Number
                || srcType == DataType.String
                || srcType == DataType.Undefined
                || srcType == DataType.Type);

            return TypeManager.GetTypeById((int)srcType);
        }

        public bool Equals(TypeDescriptor other)
        {
            return other.ID == this.ID;
        }
    }

    [Serializable]
    struct SymbolBinding
    {
        public int CodeIndex;
        public int ContextIndex;
    }

    public enum SymbolType
    {
        Variable,
        ContextProperty
    }

    public struct VariableInfo
    {
        public int Index;
        public string Identifier;
        public SymbolType Type;
    }

    struct VariableBinding
    {
        public SymbolType type;
        public SymbolBinding binding;
    }
}
