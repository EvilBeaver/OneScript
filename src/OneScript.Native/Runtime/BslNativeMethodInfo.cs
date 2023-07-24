/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    public class BslNativeMethodInfo : BslScriptMethodInfo
    {
        private CallableMethod _callable;

        public BslNativeMethodInfo()
        {
            _callable = new CallableMethod(this);
        }
        
        public void SetImplementation(LambdaExpression lambda)
        {
            Implementation = lambda;
            _callable.Compile();
        }

        private CallableMethod CreateCallable()
        {
            return new CallableMethod(this);
        }

        internal CallableMethod GetCallable() => _callable;

        public LambdaExpression Implementation { get; private set; }

        public bool IsInstance { get; internal set; }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            // FIXME: Из стековой машины дефолтные значения могут прийти, как null или Skipped
            // здесь мы принудительно проставляем пропущенные параметры
            var bslArguments = new BslValue[parameters.Length];
            for (int i = 0; i < bslArguments.Length; i++)
            {
                var param = parameters[i];
                if (param == null || param == BslSkippedParameterValue.Instance)
                {
                    if (_parameters[i].HasDefaultValue)
                        bslArguments[i] = (BslValue)_parameters[i].DefaultValue;
                    else
                        throw RuntimeException.MissedArgument();
                }
                else
                {
                    bslArguments[i] = (BslValue)param;
                }
            }

            return _callable.Invoke(obj, bslArguments);
        }
        
    }
}