using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oscript.DebugServer
{
    internal class FinalState : ConsoleDebuggerState
    {
        public FinalState(InteractiveDebugController controller) : base(controller)
        {
        }

        public override void Enter()
        {
        }
    }
}
