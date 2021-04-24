/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;

namespace OneScript.Types
{
    public interface ITypeFactory
    {
        //todo сейчас работает в лоб, через текущую фабрику. Сам не вызывает статически методы-конструкторы

        IValue Activate(TypeActivationContext context, IValue[] arguments);
    }
}