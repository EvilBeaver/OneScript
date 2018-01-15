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
using System.Threading.Tasks;

namespace oscript.DebugServer
{
    internal abstract class DebuggerState
    {
        private readonly List<DebuggerCommandDescription> _commands;
        public DebuggerState()
        {
            _commands = new List<DebuggerCommandDescription>();
        }

        public virtual void ExecuteCommand(DebuggerCommands command, object[] arguments)
        {
            RunCommand(command, arguments);
        }
        
        public virtual void Enter()
        {
            
        }

        public string Prompt { get; protected set; }

        public IEnumerable<DebuggerCommandDescription> Commands => _commands;

        protected void AddCommand(DebuggerCommandDescription cmd)
        {
            _commands.Add(cmd);
        }

        protected void RunCommand(DebuggerCommands cmd, object[] args)
        {
            var cmdDescr = Commands.First(x => x.Command == cmd);
            cmdDescr.Action(args);
        }

    }
}
