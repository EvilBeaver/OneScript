using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.DebugProtocol
{
    public interface IVariableLocator : IEnumerable<Variable>
    {
        int Count { get; }
        Variable this[int index] { get; }

        void Hydrate(IDebuggerService process);

        IVariableLocator CreateChildLocator(int variableIndex);
    }

    public class VariableLocator : IVariableLocator
    {

        private int[] _path;
        private readonly int _stackFrameIndex;

        private Variable[] _variables;

        private VariableLocator(VariableLocator parent, int variableIndex)
        {
            _stackFrameIndex = parent._stackFrameIndex;
            _path = new int[parent._path.Length + 1];
            Array.Copy(parent._path, _path, parent._path.Length);
            _path[parent._path.Length] = variableIndex;
        }

        public VariableLocator(int stackFrameIndex)
        {
            _stackFrameIndex = stackFrameIndex;
            _path = new int[0];
        }

        public VariableLocator(int stackFrameIndex, int variableIndex)
        {
            _stackFrameIndex = stackFrameIndex;
            _path = new int[] { variableIndex };
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
            var variables = process.GetVariables(_stackFrameIndex, _path);
            _variables = variables;
        }

        IVariableLocator IVariableLocator.CreateChildLocator(int variableIndex)
        {
            return new VariableLocator(this, variableIndex);
        }
    }
}
