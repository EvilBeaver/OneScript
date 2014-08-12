using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public static class MethodUsageExtractor
    {
        public static MethodUsageData[] Extract(IRuntimeContextInstance context)
        {
            int count = context.GetMethodsCount();
            if (count == 0)
                return new MethodUsageData[0];

            MethodUsageData[] result = new MethodUsageData[count];

            for (int i = 0; i < count; i++)
            {
                MethodParameter[] parameters = new MethodParameter[context.GetParametersCount(i)];

                for (int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
                {
                    parameters[paramIdx].IsByValue = true;
                    
                    IValue defValDummy;
                    if (context.GetDefaultValue(i, paramIdx, out defValDummy))
                        parameters[paramIdx].IsOptional = true;

                }
                
                MethodUsageData md;
                if (context.HasReturnValue(i))
                    md = MethodUsageData.CreateFunction(new ParametersList(parameters));
                else
                    md = MethodUsageData.CreateProcedure(new ParametersList(parameters));


                result[i] = md;

            }

            return result;
        }
    }
}
