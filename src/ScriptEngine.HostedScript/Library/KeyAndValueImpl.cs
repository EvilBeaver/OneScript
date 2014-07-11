using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("КлючИЗначение", "KeyAndValue")]
    class KeyAndValueImpl : AutoContext<KeyAndValueImpl>
    {
        private IValue _key;
        private IValue _value;

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
