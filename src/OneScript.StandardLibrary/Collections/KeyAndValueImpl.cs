/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Diagnostics;
using OneScript.Contexts;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("КлючИЗначение", "KeyAndValue")]
    public class KeyAndValueImpl : AutoContext<KeyAndValueImpl>
    {
        public KeyAndValueImpl(IValue key, IValue value)
        {
            Debug.Assert(!(key is IValueReference));
            Debug.Assert(!(value is IValueReference));
            
            Key = key;
            Value = value;
        }

        [ContextProperty("Ключ", "Key")]
        public IValue Key { get; }

        [ContextProperty("Значение", "Value")]
        public IValue Value { get; }

        public override IValue GetPropValue(int propNum)
        {
            return propNum == 0 ? Key : Value;
        }

    }
}
