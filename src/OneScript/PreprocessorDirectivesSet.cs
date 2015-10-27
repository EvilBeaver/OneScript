using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public class PreprocessorDirectivesSet : IEnumerable<string>
    {
        private HashSet<string> _values = new HashSet<string>();

        public void Set(string directive)
        {
            _values.Add(directive);
        }

        public void Set(IEnumerable<string> directives)
        {
            foreach (var item in directives)
            {
                _values.Add(item);
            }
        }
        public void Unset(string directive)
        {
            _values.Remove(directive);
        }

        public bool IsSet(string directive)
        {
            return _values.Contains(directive);
        }

        public void Clear()
        {
            _values.Clear();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
