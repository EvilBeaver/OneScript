using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class StringCodeSource : IScriptSource
    {
        string _code;

        public StringCodeSource(string code)
        {
            _code = code;
        }

        public string SourceName
        {
            get;
            set;
        }

        public string GetCode()
        {
            return _code;
        }
    }
}
