/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;

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
            if (_index >= _args.Length)
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

        public CmdLineParam Parse(string param)
        {
            var paramValue = new CmdLineParam();
            var equality = param.IndexOf('=');
            if (equality == -1)
            {
                paramValue.Name = param;
            }
            else
            {
                paramValue.Name = param.Substring(0, equality);
                if (param.Length > equality + 1)
                    paramValue.Value = param.Substring(equality + 1);
                else
                    paramValue.Value = string.Empty;
            }

            return paramValue;
        }
    }
}
