using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Runtime
{
    public static class MethodSignatureExtractor
    {
        public static MethodSignatureData[] Extract(IRuntimeContextInstance context)
        {
            int count = context.GetMethodsCount();
            if (count == 0)
                return new MethodSignatureData[0];

            MethodSignatureData[] result = new MethodSignatureData[count];

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
                
                MethodSignatureData md;
                if (context.HasReturnValue(i))
                    md = MethodSignatureData.CreateFunction(new ParametersList(parameters));
                else
                    md = MethodSignatureData.CreateProcedure(new ParametersList(parameters));


                result[i] = md;

            }

            return result;
        }
    }
}
