using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Compiler
{
    class SourceCodeIndexer
    {
        private string _code;
        private IList<int> _lineBounds;

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
