/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Sources
{
    public class StringCodeSource : ICodeSource
    {
        private readonly string _src;

        public StringCodeSource(string src)
        {
            _src = src;
        }
        
        public string Location => $"<string {_src.GetHashCode():X8}>" ;
        
        public string GetSourceCode()
        {
            return _src;
        }
    }
}