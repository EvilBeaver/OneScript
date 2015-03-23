using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine
{
    interface IDirectiveResolver
    {
        bool Resolve(string directive, string value);
    }
}
