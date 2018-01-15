using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OneScript.DebugProtocol.FSM;

namespace oscript.DebugServer
{
    public class DebuggerFSM
    {
        private class StateRecord
        {
            public DebuggerState state;
            public string command;
            public DebuggerState nextState;
        }

        private List<StateRecord> _statesTable = new List<StateRecord>();
        
        public DebuggerState CurrentState { get; private set; }
        
        public void AddTransition(DebuggerState state, string command, DebuggerState nextState)
        {
            var record = new StateRecord()
            {
                state = state,
                command = command,
                nextState = nextState
            };

            _statesTable.Add(record);
        }

        public void Start()
        {
            CurrentState = _statesTable[0].state;
            CurrentState.Enter();
        }

        public void DispatchCommand(string command, object[] arguments)
        {
            var availableTransition = _statesTable.FirstOrDefault(x => x.command == command && x.state == CurrentState);
            if (availableTransition == null)
            {
                throw new InvalidDebuggerCommandException();
            }
            
            CurrentState.ExecuteCommand(command, arguments);
            CurrentState = availableTransition.nextState;
            CurrentState.Enter();
        }
    }

    internal class InvalidDebuggerCommandException : ApplicationException
    {
        public InvalidDebuggerCommandException() : base("Команда не поддерживается в данном режиме")
        {
            
        }
    }
}
