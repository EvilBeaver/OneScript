/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace OneScript.DebugProtocol
{
    [DataContract, JsonObject, Serializable]
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

        public int ThreadId { get; set; }

        int IVariableLocator.Count
        {
            get
            {
                if (Variables == null)
                    return 0;
                
                return Variables.Length;
            }
        }

        Variable IVariableLocator.this[int index]
        {
            get
            {
                if (Variables == null)
                    throw new ArgumentOutOfRangeException();
                
                return Variables[index];
            }
        }

        IEnumerator<Variable> IEnumerable<Variable>.GetEnumerator()
        {
            if (Variables == null)
            {
                return EmptyEnumerator<Variable>.Instance;
            }

            return ((IEnumerable<Variable>)Variables).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IVariableLocator) this).GetEnumerator();
        }

        void IVariableLocator.Hydrate(IDebuggerService process)
        {
            if (Variables != null)
                return;
            var variables = process.GetVariables(ThreadId, Index, Array.Empty<int>());
            Variables = variables;
        }

        public IVariableLocator CreateChildLocator(int variableIndex)
        {
            return new VariableLocator(ThreadId, Index, variableIndex);
        }
    }
    
    internal class EmptyEnumerator<T> : IEnumerator<T>
    {
        public static EmptyEnumerator<T> Instance { get; } = new EmptyEnumerator<T>();
        
        private EmptyEnumerator()
        {
        }

        public bool MoveNext()
        {
            return false;
        }

        public void Reset()
        {
        }

        public T Current { get; } = default;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}
