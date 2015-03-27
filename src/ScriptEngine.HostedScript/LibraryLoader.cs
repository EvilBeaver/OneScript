using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript
{
    class LibraryLoader : IDirectiveResolver
    {
        public LibraryLoader(RuntimeEnvironment env)
        {

        }

        public bool Resolve(string directive, string value)
        {
            throw new NotImplementedException();
        }
    }
}
