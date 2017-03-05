using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine
{
    internal class VariablesFrame : IList<string>
    {
        private readonly List<string> _data;

        public VariablesFrame()
        {
            _data = new List<string>();
        }

        public VariablesFrame(IEnumerable<string> src)
        {
            _data = new List<string>(src);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _data).GetEnumerator();
        }

        public void Add(string item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(string item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return _data.Remove(item);
        }

        public int Count
        {
            get { return _data.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(string item)
        {
            return _data.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            _data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }

        public string this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }
    }
}
