/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugServer
{
    public class Handles<T>
    {
        private const int START_HANDLE = 1000;

        private int _nextHandle;
        private Dictionary<int, T> _handleMap;

        public Handles()
        {
            _nextHandle = START_HANDLE;
            _handleMap = new Dictionary<int, T>();
        }

        public void Reset()
        {
            _nextHandle = START_HANDLE;
            _handleMap.Clear();
        }

        public int Create(T value)
        {
            var handle = _nextHandle++;
            _handleMap[handle] = value;
            return handle;
        }

        public bool TryGet(int handle, out T value)
        {
            if (_handleMap.TryGetValue(handle, out value))
            {
                return true;
            }
            return false;
        }

        public T Get(int handle, T dflt)
        {
            T value;
            if (_handleMap.TryGetValue(handle, out value))
            {
                return value;
            }
            return dflt;
        }
    }
}
