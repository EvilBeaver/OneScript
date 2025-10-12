/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using ScriptEngine.Machine;

namespace OneScript.Contexts
{
    public interface IContextValueConverter<TClr>
    {
        IValue ToIValue(TClr obj);

        TClr ToClr(IValue obj);
        
        public static bool ImplementsIt(Type type)   
            => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IContextValueConverter<>));
    }
}
