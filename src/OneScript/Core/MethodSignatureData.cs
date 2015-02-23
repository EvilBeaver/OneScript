using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class MethodSignatureData
    {
        private ParametersList _paramList;
        private bool _isFunction;

        private MethodSignatureData()
        {

        }

        public bool IsFunction
        {
            get
            {
                return _isFunction;
            }
        }

        public ParametersList Parameters
        {
            get
            {
                return _paramList;
            }
        }

        public static MethodSignatureData CreateProcedure(ParametersList parameters)
        {
            return new MethodSignatureData()
            {
                _paramList = parameters,
                _isFunction = false
            };
        }

        public static MethodSignatureData CreateFunction(ParametersList parameters)
        {
            return new MethodSignatureData()
            {
                _paramList = parameters,
                _isFunction = true
            };
        }

        public static MethodSignatureData CreateProcedure(int paramCount)
        {
            return CreateProcedure(ParametersList.CreateDefault(paramCount));
        }

        public static MethodSignatureData CreateFunction(int paramCount)
        {
            return CreateFunction(ParametersList.CreateDefault(paramCount));
        }

    }

}
