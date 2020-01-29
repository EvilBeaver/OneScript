/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("КлючИЗначение", "KeyAndValue")]
    public class KeyAndValueImpl : AutoContext<KeyAndValueImpl>
    {
        private readonly IValue _key;
        private readonly IValue _value;

        public KeyAndValueImpl(IValue key, IValue value)
        {
            _key = key;
            _value = value;
        }

        [ContextProperty("Ключ", "Key")]
        public IValue Key 
        {
            get
            {
                return _key;
            }
        }

        [ContextProperty("Значение", "Value")]
        public IValue Value
        {
            get
            {
                return _value;
            }
        }

        public override IValue GetPropValue(int propNum)
        {
            return propNum == 0 ? _key : _value;
        }

    }
}
