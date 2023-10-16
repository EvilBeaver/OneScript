﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Exceptions;

namespace ScriptEngine.Machine
{
    [Obsolete]
    public static class RciHelperExtensions
    {
        public static IEnumerable<BslMethodInfo> GetMethods(this IRuntimeContextInstance context)
        {
            var methods = new BslMethodInfo[context.GetMethodsCount()];
            for (int i = 0; i < methods.Length; i++)
            {
                methods[i] = context.GetMethodInfo(i);
            }

            return methods;
        }

        public static IEnumerable<BslPropertyInfo> GetProperties(this IRuntimeContextInstance context)
        {
            var infos = new BslPropertyInfo[context.GetPropCount()];
            for (var i = 0; i < infos.Length; i++)
            {
                infos[i] = context.GetPropertyInfo(i);
            }

            return infos;
        }

        public static IValue GetPropValue(this IRuntimeContextInstance context, string propName)
        {
            int propNum = context.GetPropertyNumber(propName);

            if (propNum == -1)
            {
                throw RuntimeException.InvalidArgumentValue(propName);
            }

            return context.GetPropValue(propNum);
        }
        
        public static void SetPropValue(this IRuntimeContextInstance context, string propName, IValue value)
        {
            int propNum = context.GetPropertyNumber(propName);

            if (propNum == -1)
            {
                throw RuntimeException.InvalidArgumentValue(propName);
            }

            context.SetPropValue(propNum, value);
        }
    }
}