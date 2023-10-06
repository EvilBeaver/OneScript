/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Language;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Уровень косвенности между нативным рантаймом и ExceptionInfoContext, который тот не видит. 
    /// </summary>
    public class ExceptionInfoFactory : IExceptionInfoFactory
    {
        public BslObjectValue GetExceptionInfo(Exception exception)
        {
            if (exception == null)
            {
                return ExceptionInfoContext.EmptyExceptionInfo();
            }
            
            if (exception is ScriptException script)
                return new ExceptionInfoContext(script);
            
            return new ExceptionInfoContext(new ExternalSystemException(exception));
        }

        public string GetExceptionDescription(IRuntimeContextInstance exceptionInfo)
        {
            if (exceptionInfo == null)
            {
                return "";
            }
            
            var info = (ExceptionInfoContext)exceptionInfo;
            return info.MessageWithoutCodeFragment;
        }

        public Exception Raise(object raiseValue)
        {
            return raiseValue switch
            {
                ExceptionInfoContext { IsErrorTemplate: true } excInfo => 
                    new ParametrizedRuntimeException(excInfo.Description, excInfo.Parameters),
                BslValue bslVal => new RuntimeException(bslVal.AsString()),
                _ => new RuntimeException(raiseValue.ToString())
            };
        }
    }
}