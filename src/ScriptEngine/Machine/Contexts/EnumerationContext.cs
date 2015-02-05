using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class EnumerationContext : PropertyNameIndexAccessor
    {
        private Dictionary<string, int> _nameIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private List<EnumerationValue> _values = new List<EnumerationValue>();
        private TypeDescriptor _valuesType;

        public EnumerationContext(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) : base(typeRepresentation)
        {
            _valuesType = valuesType;
        }

        public void AddValue(string name, EnumerationValue val)
        {
            System.Diagnostics.Debug.Assert(val != null);

            if(!ScriptEngine.Utils.IsValidIdentifier(name))
            {
                throw new ArgumentException("Name must be a valid identifier", "name");
            }

            int id = _values.Count;
            _nameIndexes.Add(name, id);
            _values.Add(val);
            
            val.ValuePresentation = name;
            
        }

        public TypeDescriptor ValuesType
        {
            get
            {
                return _valuesType;
            }
        }

        public EnumerationValue this[string name]
        {
            get
            {
                int id = FindProperty(name);
                return _values[id];
            }
        }

        public int IndexOf(EnumerationValue enumVal)
        {
            return _values.IndexOf(enumVal);
        }

        public override int FindProperty(string name)
        {
            int id;
            if (_nameIndexes.TryGetValue(name, out id))
            {
                return id;
            }
            else
                return base.FindProperty(name);
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            return _values[propNum];
        }

        protected IList<EnumerationValue> ValuesInternal
        {
            get
            {
                return _values;
            }
        }
    }
}
