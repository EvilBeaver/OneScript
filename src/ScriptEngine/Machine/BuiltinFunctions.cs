using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    static class BuiltinFunctions
    {
        static Dictionary<OperationCode, ParameterDefinition[]> _paramInfoCache = new Dictionary<OperationCode,ParameterDefinition[]>();
        static Dictionary<OperationCode, Func<ParameterDefinition[]>> _paramInfoGenerators = null;

        static BuiltinFunctions()
        {
            InitParametersInfo();
        }

        public static ParameterDefinition[] ParametersInfo(OperationCode funcOpcode)
        {
            ParameterDefinition[] info;
            if(!_paramInfoCache.TryGetValue(funcOpcode, out info))
            {
                info = _paramInfoGenerators[funcOpcode]();
                _paramInfoCache.Add(funcOpcode, info);
            }

            return info;
        }

        private static void InitParametersInfo()
        {
            var map = new Dictionary<OperationCode, Func<ParameterDefinition[]>>();

            // conversion
            map.Add(OperationCode.Bool, SingleDefaultParamInfo);
            map.Add(OperationCode.Str, SingleDefaultParamInfo);
            map.Add(OperationCode.Number, SingleDefaultParamInfo);
            map.Add(OperationCode.Date, DateFunctionParamInfo);
            map.Add(OperationCode.Type, SingleDefaultParamInfo);
            map.Add(OperationCode.ValType, SingleDefaultParamInfo);
            // string
            map.Add(OperationCode.StrLen, SingleDefaultParamInfo);
            map.Add(OperationCode.TrimL, SingleDefaultParamInfo);
            map.Add(OperationCode.TrimR, SingleDefaultParamInfo);
            map.Add(OperationCode.TrimLR, SingleDefaultParamInfo);
            map.Add(OperationCode.Left, TwoDefaultParamsInfo);
            map.Add(OperationCode.Right, TwoDefaultParamsInfo);
            map.Add(OperationCode.Mid, StrMidParamsInfo);
            map.Add(OperationCode.StrPos, TwoDefaultParamsInfo);
            map.Add(OperationCode.UCase, SingleDefaultParamInfo);
            map.Add(OperationCode.LCase, SingleDefaultParamInfo);
            map.Add(OperationCode.Chr, SingleDefaultParamInfo);
            map.Add(OperationCode.ChrCode, SingleDefaultParamInfo);
            map.Add(OperationCode.EmptyStr, SingleDefaultParamInfo);
            map.Add(OperationCode.StrReplace, StrReplaceParamInfo);
            // date
            map.Add(OperationCode.Year, SingleDefaultParamInfo);
            map.Add(OperationCode.Month, SingleDefaultParamInfo);
            map.Add(OperationCode.Day, SingleDefaultParamInfo);
            map.Add(OperationCode.Hour, SingleDefaultParamInfo);
            map.Add(OperationCode.Minute, SingleDefaultParamInfo);
            map.Add(OperationCode.Second, SingleDefaultParamInfo);
            map.Add(OperationCode.BegOfYear, SingleDefaultParamInfo);
            map.Add(OperationCode.BegOfMonth, SingleDefaultParamInfo);
            map.Add(OperationCode.BegOfDay, SingleDefaultParamInfo);
            map.Add(OperationCode.BegOfHour, SingleDefaultParamInfo);
            map.Add(OperationCode.BegOfMinute, SingleDefaultParamInfo);
            map.Add(OperationCode.BegOfQuarter, SingleDefaultParamInfo);
            map.Add(OperationCode.EndOfYear, SingleDefaultParamInfo);
            map.Add(OperationCode.EndOfMonth, SingleDefaultParamInfo);
            map.Add(OperationCode.EndOfDay, SingleDefaultParamInfo);
            map.Add(OperationCode.EndOfHour, SingleDefaultParamInfo);
            map.Add(OperationCode.EndOfMinute, SingleDefaultParamInfo);
            map.Add(OperationCode.EndOfQuarter, SingleDefaultParamInfo);
            map.Add(OperationCode.WeekOfYear, SingleDefaultParamInfo);
            map.Add(OperationCode.DayOfYear, SingleDefaultParamInfo);
            map.Add(OperationCode.DayOfWeek, SingleDefaultParamInfo);
            map.Add(OperationCode.AddMonth, TwoDefaultParamsInfo);
            map.Add(OperationCode.CurrentDate, NoParamsInfo);
            // number
            map.Add(OperationCode.Integer, SingleDefaultParamInfo);
            map.Add(OperationCode.Round, RoundParamsInfo);
            map.Add(OperationCode.Log, SingleDefaultParamInfo);
            map.Add(OperationCode.Log10, SingleDefaultParamInfo);
            map.Add(OperationCode.Sin, SingleDefaultParamInfo);
            map.Add(OperationCode.Cos, SingleDefaultParamInfo);
            map.Add(OperationCode.Tan, SingleDefaultParamInfo);
            map.Add(OperationCode.ASin, SingleDefaultParamInfo);
            map.Add(OperationCode.ACos, SingleDefaultParamInfo);
            map.Add(OperationCode.ATan, SingleDefaultParamInfo);
            map.Add(OperationCode.Exp, SingleDefaultParamInfo);
            map.Add(OperationCode.Pow, TwoDefaultParamsInfo);
            map.Add(OperationCode.Sqrt, SingleDefaultParamInfo);
            map.Add(OperationCode.Format, TwoDefaultParamsInfo);
            // special
            map.Add(OperationCode.Question, QuestionParamInfo);
            map.Add(OperationCode.ExceptionInfo, NoParamsInfo);
            map.Add(OperationCode.ExceptionDescr, NoParamsInfo);
            map.Add(OperationCode.ModuleInfo, NoParamsInfo);

            _paramInfoGenerators = map;
        }

        private static ParameterDefinition[] SingleDefaultParamInfo()
        {
            return new ParameterDefinition[] 
            {
                new ParameterDefinition() {IsByValue = true}
            };
        }

        private static ParameterDefinition[] DateFunctionParamInfo()
        {
            var optionalParam = new ParameterDefinition()
            {
                HasDefaultValue = true,
                IsByValue = true
            };

            return new ParameterDefinition[6] 
            {
                new ParameterDefinition(){IsByValue = true},
                optionalParam,
                optionalParam,
                optionalParam,
                optionalParam,
                optionalParam
            };
        }

        private static ParameterDefinition[] QuestionParamInfo()
        {
            return MandatoryParamInfo(3);
        }

        private static ParameterDefinition[] MandatoryParamInfo(int amount)
        {
            var mandatoryArg = new ParameterDefinition()
            {
                IsByValue = true
            };

            var result = new ParameterDefinition[amount];
            for (int i = 0; i < amount; i++)
            {
                result[i] = mandatoryArg;
            }

            return result;
        }

        private static ParameterDefinition[] TwoDefaultParamsInfo()
        {
            return MandatoryParamInfo(2);
        }

        private static ParameterDefinition[] NoParamsInfo()
        {
            return new ParameterDefinition[0];
        }

        private static ParameterDefinition[] StrMidParamsInfo()
        {
            var optionalParam = new ParameterDefinition()
            {
                HasDefaultValue = true,
                IsByValue = true
            };

            return new ParameterDefinition[3]
                {
                    new ParameterDefinition(),
                    new ParameterDefinition(),
                    optionalParam
                };
            
        }

        private static ParameterDefinition[] StrReplaceParamInfo()
        {
            return MandatoryParamInfo(3);
        }

        private static ParameterDefinition[] RoundParamsInfo()
        {
            var optionalParam = new ParameterDefinition()
            {
                HasDefaultValue = true,
                IsByValue = true
            };

            return new ParameterDefinition[3]
                {
                    new ParameterDefinition(),
                    optionalParam,
                    optionalParam
                };
        }

    }
}
