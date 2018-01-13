using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Environment;

namespace OneScript.ASPNETHandler
{
    class AspEntryScriptSrc : ICodeSource
    {
        string _sourceDescription;

        public string Code
        {
            get { return ""; }
        }
        public AspEntryScriptSrc(string sourceDescription)
        {
            _sourceDescription = sourceDescription;
        }
        public string SourceDescription
        {
            get { return _sourceDescription; }
        }
    }
}
