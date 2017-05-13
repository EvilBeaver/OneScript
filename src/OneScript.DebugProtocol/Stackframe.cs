using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OneScript.DebugProtocol
{
    [DataContract]
    public class StackFrame : IVariableLocator
    {
        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public string MethodName { get; set; }

        [DataMember]
        public int LineNumber { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public Variable[] Variables { get; set; }

        int IVariableLocator.Count
        {
            get
            {
                ThrowIfNoVariables();

                return Variables.Length;
            }
        }

        private void ThrowIfNoVariables()
        {
            if (Variables == null)
                throw new InvalidOperationException("No variables aquired yet");
        }

        Variable IVariableLocator.this[int index]
        {
            get
            {
                ThrowIfNoVariables();
                return Variables[index];
            }
        }

        IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator()
        {
            ThrowIfNoVariables();

            return ((IEnumerable<Variable>)Variables).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IVariableLocator) this).GetEnumerator();
        }

        void IVariableLocator.Hydrate(IDebuggerService process)
        {
            var variables = process.GetVariables(Index, new int[0]);
            Variables = variables;
        }

        public IVariableLocator CreateChildLocator(int variableIndex)
        {
            return new VariableLocator(Index, variableIndex);
        }
    }
}
