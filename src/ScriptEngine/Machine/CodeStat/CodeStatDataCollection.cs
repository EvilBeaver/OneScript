using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class CodeStatDataCollection : ICollection<CodeStatData>
    {
        private List<CodeStatData> entryList = new List<CodeStatData>();

        public int Count
        {
            get
            {
                return entryList.Count();
            }
        }

        public CodeStatData this[int index]
        {
            get
            {
                return entryList[index];
            }
            set
            {
                entryList[index] = value;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false; 
            }
        }

        public void Add(CodeStatData item)
        {
            entryList.Add(item);
        }

        public void Clear()
        {
            entryList.Clear();
        }

        public bool Contains(CodeStatData item)
        {
            return entryList.Contains(item);
        }

        public void CopyTo(CodeStatData[] array, int arrayIndex)
        {
            entryList.CopyTo(array, arrayIndex);
        }

        public bool Remove(CodeStatData item)
        {
            return entryList.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return entryList.GetEnumerator();
        }

        public IEnumerator<CodeStatData> GetEnumerator()
        {
            return entryList.GetEnumerator();
        }
    }
}
