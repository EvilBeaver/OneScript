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
        public string Code
        {
            get { return ""; }
        }

        public string SourceDescription
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "web.config"); }
        }
    }
}
