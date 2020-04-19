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
using Newtonsoft.Json;

namespace oscript
{
    class CmdLineHelper
    {
        private string[] _args;
        private int _index = -1;

        public CmdLineHelper(string[] args)
        {
            _args = args;
        }

        public string Next()
        {
            _index++;
            if (_index == _args.Length)
                return null;

            return _args[_index];
        }

        public string Current()
        {
            if (_index < 0 || _index >= _args.Length)
                return null;

            return _args[_index];
        }

        public string[] Tail()
        {
            return _args.Skip(_index+1).ToArray();
        }

        public string ValueOfKey(string key, string param)
        {
            string value = null;
            var len = key.Length;
            if (param.Length > len)
            {
                value = param.Substring(len);
            }

            return value;
        }
    }
}
