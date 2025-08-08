/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;

namespace OneScript.Contexts
{
    public abstract class ContextValueConverter<TClr>
    {
        public abstract IValue ToIValue(TClr obj);

        public abstract TClr ToClr(IValue obj);
    }
}
