using OneScript.Core;

namespace OneScript.Runtime
{
    class ContextPropertyVariable : IVariable
    {
        IRuntimeContextInstance _ctx;
        int _index;

        public ContextPropertyVariable(IRuntimeContextInstance context, int propertyIndex)
        {
            _ctx = context;
            _index = propertyIndex;
        }

        public IValue Value
        {
            get
            {
                return _ctx.GetPropertyValue(_index);
            }
            set
            {
                _ctx.SetPropertyValue(_index, value);
            }
        }

        public IValue Dereference()
        {
            return Value;
        }

        public bool Equals(IValue other)
        {
            return this.Value.Equals(other);
        }
    }
}
