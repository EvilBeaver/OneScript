using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oscript.DebugServer
{
    internal class DebuggerFSM
    {
        private class StateRecord
        {
            public DebuggerState state;
            public DebuggerCommands command;
            public DebuggerState nextState;
        }

        private List<StateRecord> _statesTable = new List<StateRecord>();
        public DebuggerState CurrentState { get; private set; }
        
        public void AddState(DebuggerState state, DebuggerCommands command, DebuggerState nextState)
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

        public void DispatchCommand(DebuggerCommands command, string[] arguments)
        {
            var availableTransition = _statesTable.FirstOrDefault(x => x.command == command && x.state == CurrentState);
            if (availableTransition == null)
            {
                throw new InvalidDebuggerCommandException();
            }

            try
            {
                availableTransition.state.ExecuteCommand(command, arguments);
            }
            catch (InvalidDebuggerCommandException e)
            {
                Output.WriteLine(e.Message);
            }

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
