/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
namespace OneScriptDocumenter
{
    class CommandLineArgs
    {
        readonly string[] _args;
        int _index = 0;

        public CommandLineArgs(string[] argsArray)
        {
            _args = argsArray;
        }

        public string Next()
        {
            if (_index >= _args.Length)
                return null;

            return _args[_index++];
        }
    }
}
