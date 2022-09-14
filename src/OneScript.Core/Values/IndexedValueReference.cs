using System;
using OneScript.Contexts;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public class IndexedValueReference : IValueReference
    {
        private readonly IRuntimeContextInstance _context;
        private readonly BslValue _index;

        public IndexedValueReference(IRuntimeContextInstance context, BslValue index)
        {
            _context = context;
            _index = index;
        }

        public IValue Value
        {
            get => (BslValue)_context.GetIndexedValue(_index);
            set => _context.SetIndexedValue(_index, value);
        }

        public bool Equals(IValueReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!(other is IndexedValueReference idxRef))
                return false;
            
            return Equals(_context, idxRef._context) && Equals(_index, idxRef._index);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IndexedValueReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_context, _index);
        }
    }
}