/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    [Serializable]
    internal class VariablesFrame : IList<VariableInfo>
    {
        private readonly List<VariableInfo> _data;

        public VariablesFrame()
        {
            _data = new List<VariableInfo>();
        }

        public VariablesFrame(IEnumerable<VariableInfo> src)
        {
            _data = new List<VariableInfo>(src);
        }

        public IEnumerator<VariableInfo> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _data).GetEnumerator();
        }

        public void Add(VariableInfo item)
        {
            _data.Add(item);
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(VariableInfo item)
        {
            return _data.Contains(item);
        }

        public void CopyTo(VariableInfo[] array, int arrayIndex)
        {
            _data.CopyTo(array, arrayIndex);
        }

        public bool Remove(VariableInfo item)
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

        public int IndexOf(VariableInfo item)
        {
            return _data.IndexOf(item);
        }

        public void Insert(int index, VariableInfo item)
        {
            _data.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _data.RemoveAt(index);
        }

        public VariableInfo this[int index]
        {
            get { return _data[index]; }
            set { _data[index] = value; }
        }
    }
}
