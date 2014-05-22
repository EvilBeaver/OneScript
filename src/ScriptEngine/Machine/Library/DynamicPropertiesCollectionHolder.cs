using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    abstract class DynamicPropertiesCollectionHolder : PropertyNameIndexAccessor
    {
        private Dictionary<string, int> _propNumbers;
        
        public DynamicPropertiesCollectionHolder()
        {
            _propNumbers = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        }
 
        protected int RegisterProperty(string name)
        {
            if(_propNumbers.ContainsKey(name))
            {
                return _propNumbers[name];
            }
            else
            {
                if (!IsValidIdentifier(name))
                {
                    throw new RuntimeException("Wrong argument value");
                }

                var idx = _propNumbers.Count;
                _propNumbers.Add(name, idx);
                return idx;
            }
        }

        private bool IsValidIdentifier(string name)
        {
            if (name == null || name.Length == 0)
                return false;

            if (!(Char.IsLetter(name[0]) || name[0] == '_'))
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!Char.IsLetterOrDigit(name[i]))
                    return false;
            }

            return true;
        }

        protected void RemoveProperty(string name)
        {
            _propNumbers.Remove(name);
        }

        protected void ReorderPropertyNumbers()
        {
            var sorted = _propNumbers.OrderBy(x=>x.Value).Select(x=>x.Key).ToArray();
            _propNumbers.Clear();
            for (int i = 0; i < sorted.Length; i++)
            {
                _propNumbers.Add(sorted[i], i);
            }
        }

        protected void ClearProperties()
        {
            _propNumbers.Clear();
        }

        protected IEnumerable<KeyValuePair<string, int>> GetProperties()
        {
            return _propNumbers.AsEnumerable();
        }

        #region IRuntimeContextInstance Members

        public override bool IsIndexed
        {
            get { return true; }
        }

        public override int FindProperty(string name)
        {
            try
            {
                return _propNumbers[name];
            }
            catch (KeyNotFoundException)
            {
                throw RuntimeException.PropNotFoundException(name);
            }
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override bool IsPropWritable(int propNum)
        {
            return true;
        }

        #endregion

    }
}
