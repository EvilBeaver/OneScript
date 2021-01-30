/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ScriptEngine.Machine
{
    public static class BuiltinFunctions
    {
        static readonly Dictionary<OperationCode, ParameterDefinition[]> _paramInfoCache = new Dictionary<OperationCode,ParameterDefinition[]>();

        private static readonly ParameterDefinition MANDATORY_BYVAL = new ParameterDefinition { IsByValue = true };
        private static readonly ParameterDefinition OPTIONAL_BYVAL = new ParameterDefinition { IsByValue = true, HasDefaultValue = true };
        
        private const int BUILTIN_OPCODES_INDEX = (int)OperationCode.Eval;

        static BuiltinFunctions()
        {
            InitParametersInfo();
        }

        public static ParameterDefinition[] ParametersInfo(OperationCode funcOpcode)
        {
            return _paramInfoCache[funcOpcode];
        }

        public static OperationCode[] GetOperationCodes()
        {
            var values = Enum.GetValues(typeof(OperationCode));
            var result = new OperationCode[values.Length - BUILTIN_OPCODES_INDEX];
            for (int i = BUILTIN_OPCODES_INDEX, j = 0; i < values.Length; i++, j++)
            {
                result[j] = (OperationCode)values.GetValue(i);
            }

            return result;
        }

        private static void InitParametersInfo()
        {
            // conversion
            AddFunc(OperationCode.Eval,     MANDATORY_BYVAL);
            AddFunc(OperationCode.Bool,     MANDATORY_BYVAL);
            AddFunc(OperationCode.Str,      MANDATORY_BYVAL);
            AddFunc(OperationCode.Number,   MANDATORY_BYVAL);
            AddFunc(OperationCode.Date,     MANDATORY_BYVAL, OPTIONAL_BYVAL, OPTIONAL_BYVAL, OPTIONAL_BYVAL, OPTIONAL_BYVAL, OPTIONAL_BYVAL);
            AddFunc(OperationCode.Type,     MANDATORY_BYVAL);
            AddFunc(OperationCode.ValType,  MANDATORY_BYVAL);
            
            // string
            AddFunc(OperationCode.StrLen,   MANDATORY_BYVAL);
            AddFunc(OperationCode.TrimL,    MANDATORY_BYVAL);
            AddFunc(OperationCode.TrimR,    MANDATORY_BYVAL);
            AddFunc(OperationCode.TrimLR,   MANDATORY_BYVAL);
            AddFunc(OperationCode.Left,     MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.Right,    MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.Mid,      MANDATORY_BYVAL, MANDATORY_BYVAL, OPTIONAL_BYVAL);
            AddFunc(OperationCode.StrPos,   MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.UCase,    MANDATORY_BYVAL);
            AddFunc(OperationCode.LCase,    MANDATORY_BYVAL);
            AddFunc(OperationCode.TCase,    MANDATORY_BYVAL);
            AddFunc(OperationCode.Chr,      MANDATORY_BYVAL);
            AddFunc(OperationCode.ChrCode,  MANDATORY_BYVAL, OPTIONAL_BYVAL);
            AddFunc(OperationCode.EmptyStr, MANDATORY_BYVAL);
            AddFunc(OperationCode.StrReplace,    MANDATORY_BYVAL, MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.StrGetLine,    MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.StrLineCount,  MANDATORY_BYVAL);
            AddFunc(OperationCode.StrEntryCount, MANDATORY_BYVAL, MANDATORY_BYVAL);
            
            // date
            AddFunc(OperationCode.Year,         MANDATORY_BYVAL);
            AddFunc(OperationCode.Month,        MANDATORY_BYVAL);
            AddFunc(OperationCode.Day,          MANDATORY_BYVAL);
            AddFunc(OperationCode.Hour,         MANDATORY_BYVAL);
            AddFunc(OperationCode.Minute,       MANDATORY_BYVAL);
            AddFunc(OperationCode.Second,       MANDATORY_BYVAL);
            AddFunc(OperationCode.BegOfYear,    MANDATORY_BYVAL);
            AddFunc(OperationCode.BegOfMonth,   MANDATORY_BYVAL);
            AddFunc(OperationCode.BegOfDay,     MANDATORY_BYVAL);
            AddFunc(OperationCode.BegOfHour,    MANDATORY_BYVAL);
            AddFunc(OperationCode.BegOfMinute,  MANDATORY_BYVAL);
            AddFunc(OperationCode.BegOfQuarter, MANDATORY_BYVAL);
            AddFunc(OperationCode.EndOfYear,    MANDATORY_BYVAL);
            AddFunc(OperationCode.EndOfMonth,   MANDATORY_BYVAL);
            AddFunc(OperationCode.EndOfDay,     MANDATORY_BYVAL);
            AddFunc(OperationCode.EndOfHour,    MANDATORY_BYVAL);
            AddFunc(OperationCode.EndOfMinute,  MANDATORY_BYVAL);
            AddFunc(OperationCode.EndOfQuarter, MANDATORY_BYVAL);
            AddFunc(OperationCode.WeekOfYear,   MANDATORY_BYVAL);
            AddFunc(OperationCode.DayOfYear,    MANDATORY_BYVAL);
            AddFunc(OperationCode.DayOfWeek,    MANDATORY_BYVAL);
            AddFunc(OperationCode.AddMonth,     MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.CurrentDate);
            
            // number
            AddFunc(OperationCode.Integer, MANDATORY_BYVAL);
            AddFunc(OperationCode.Round,   MANDATORY_BYVAL, OPTIONAL_BYVAL, OPTIONAL_BYVAL);
            AddFunc(OperationCode.Log,     MANDATORY_BYVAL);
            AddFunc(OperationCode.Log10,   MANDATORY_BYVAL);
            AddFunc(OperationCode.Sin,     MANDATORY_BYVAL);
            AddFunc(OperationCode.Cos,     MANDATORY_BYVAL);
            AddFunc(OperationCode.Tan,     MANDATORY_BYVAL);
            AddFunc(OperationCode.ASin,    MANDATORY_BYVAL);
            AddFunc(OperationCode.ACos,    MANDATORY_BYVAL);
            AddFunc(OperationCode.ATan,    MANDATORY_BYVAL);
            AddFunc(OperationCode.Exp,     MANDATORY_BYVAL);
            AddFunc(OperationCode.Pow,     MANDATORY_BYVAL, MANDATORY_BYVAL);
            AddFunc(OperationCode.Sqrt,    MANDATORY_BYVAL);
            AddFunc(OperationCode.Format,  MANDATORY_BYVAL, MANDATORY_BYVAL);
            
            // special
            AddFunc(OperationCode.ExceptionInfo);
            AddFunc(OperationCode.ExceptionDescr);
            AddFunc(OperationCode.ModuleInfo);
        }

        private static void AddFunc(OperationCode opCode, params ParameterDefinition[] parameters)
        {
            _paramInfoCache[opCode] = parameters;
        }
    }
}
