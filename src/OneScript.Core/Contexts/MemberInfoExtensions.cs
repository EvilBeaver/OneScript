/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Reflection;

namespace OneScript.Contexts
{
    public static class MemberInfoExtensions
    {
        public static bool IsByRef(this ParameterInfo parameter)
        {
            if (parameter is BslParameterInfo bslParam)
                return !bslParam.ExplicitByVal;

            return parameter.ParameterType.IsByRef; 
        }

        public static bool IsFunction(this MethodInfo method)
        {
            return method.ReturnType != typeof(void);
        }
    }
}