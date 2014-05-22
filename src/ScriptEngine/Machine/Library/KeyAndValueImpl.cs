using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("КлючИЗначение")]
    class KeyAndValueImpl : DynamicPropertiesCollectionHolder
    {
        private IValue _key;
        private IValue _value;

        public KeyAndValueImpl(IValue key, IValue value)
        {
            _key = key;
            _value = value;
            RegisterProperty("Ключ");
            RegisterProperty("Значение");
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public override IValue GetPropValue(int propNum)
        {
            return propNum == 0 ? _key : _value;
        }

    }
}
