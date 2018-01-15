using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OneScript.DebugProtocol.FSM;

namespace oscript.DebugServer
{
    class ConsoleDebugCommandDescription : DebuggerCommandDescription
    {
        public string HelpString { get; set; }
    }
}
