using System;
using OneScript.Commons;
using OneScript.Contexts;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public class PropertyValueReference : IValueReference
    {
        private readonly IRuntimeContextInstance _context;
        private readonly int _contextPropertyNumber;
        
        public PropertyValueReference(IRuntimeContextInstance context, string propertyName)
        {
            _context = context;
            _contextPropertyNumber = context.GetPropertyNumber(propertyName);
        }
        
        public PropertyValueReference(IRuntimeContextInstance context, int propertyId)
        {
            _context = context;
            _contextPropertyNumber = propertyId;
        }

        public bool Equals(IValueReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!(other is PropertyValueReference prop))
                return false;
            return _context.Equals(prop._context) && _contextPropertyNumber == prop._contextPropertyNumber;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((PropertyValueReference)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_context, _contextPropertyNumber);
        }

        public IValue Value
        {
            get
            {
                if (_context.IsPropReadable(_contextPropertyNumber))
                    return (BslValue)_context.GetPropValue(_contextPropertyNumber);
                
                throw PropertyAccessException.PropIsNotReadableException("");
            }
            set
            {
                if(_context.IsPropWritable(_contextPropertyNumber))
                    _context.SetPropValue(_contextPropertyNumber, value);
                else
                    throw PropertyAccessException.PropIsNotWritableException("");
            }
        }
    }
}