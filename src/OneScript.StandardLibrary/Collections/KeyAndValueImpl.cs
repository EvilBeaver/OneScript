/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Core;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections
{
    [ContextClass("КлючИЗначение", "KeyAndValue", TypeUUID = "2F949104-FC88-4ACD-A6A5-3B6C39A9C2C5")]
    public class KeyAndValueImpl : AutoContext<KeyAndValueImpl>
    {
        private static TypeDescriptor _instanceType = typeof(KeyAndValueImpl).GetTypeFromClassMarkup();
        
        public KeyAndValueImpl(IValue key, IValue value) : base(_instanceType)
        {
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
