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

namespace ScriptEngine.Compiler
{
    class SourceCodeIndexer : ISourceCodeIndexer
    {
        private readonly string _code;
        private readonly IList<int> _lineBounds;

        public SourceCodeIndexer(string code, IList<int> lineBounds)
        {
            _code = code;
            _lineBounds = lineBounds;
        }

        public string GetCodeLine(int index)
        {
            int start = GetLineBound(index);
            int end = _code.IndexOf('\n', start);
            if (end >= 0)
            {
                return _code.Substring(start, end - start);
            }
            else
            {
                return _code.Substring(start);
            }
        }

        private int GetLineBound(int lineNumber)
        {
            return _lineBounds[lineNumber - 1];
        }

    }
}
