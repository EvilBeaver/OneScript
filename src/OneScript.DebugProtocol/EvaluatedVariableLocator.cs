/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.DebugProtocol
{
    public class EvaluatedVariableLocator : IVariableLocator
    {

        private int[] _path;
        private readonly int _stackFrameIndex;
		private readonly string _expression;
        private readonly int _threadId;

        private Variable[] _variables;

        private EvaluatedVariableLocator(EvaluatedVariableLocator parent, int variableIndex)
        {
            _stackFrameIndex = parent._stackFrameIndex;
            _path = new int[parent._path.Length + 1];
            Array.Copy(parent._path, _path, parent._path.Length);
            _path[parent._path.Length] = variableIndex;
			_expression = parent._expression;
            _threadId = parent._threadId;
        }

        public EvaluatedVariableLocator(string expression, int threadId, int stackFrameIndex)
        {
            _stackFrameIndex = stackFrameIndex;
            _path = new int[0];
			_expression = expression;
            _threadId = threadId;
        }

        public EvaluatedVariableLocator(string expression, int threadId, int stackFrameIndex, int variableIndex)
        {
            _stackFrameIndex = stackFrameIndex;
            _path = new int[] { variableIndex };
			_expression = expression;
            _threadId = threadId;
        }

        public Variable this[int index]
        {
            get
            {
                ThrowIfNoVariables();

                return _variables[index];
            }
        }
        
        public int Count
        {
            get
            {
                ThrowIfNoVariables();
                return _variables.Length;
            }
        }

        public IEnumerator<Variable> GetEnumerator()
        {
            ThrowIfNoVariables();

            return ((IEnumerable<Variable>) _variables).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ThrowIfNoVariables()
        {
            if (_variables == null)
                throw new InvalidOperationException("No variables aquired yet");
        }

        void IVariableLocator.Hydrate(IDebuggerService process)
        {
            if(_variables != null)
                return;
            var variables = process.GetEvaluatedVariables(_expression, _threadId, _stackFrameIndex, _path);
            _variables = variables;
        }

        IVariableLocator IVariableLocator.CreateChildLocator(int variableIndex)
        {
            return new EvaluatedVariableLocator(this, variableIndex);
        }
    }
}
