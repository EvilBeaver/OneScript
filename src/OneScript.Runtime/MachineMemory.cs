using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class MachineMemory : IRuntimeDataContext
    {
        private List<IRuntimeValueHolder> _fixedScopes = new List<IRuntimeValueHolder>();
        private List<IRuntimeValueHolder> _dynamicScopes = new List<IRuntimeValueHolder>();
        private int _fixedCount = 0;

        public void AddScope(IRuntimeValueHolder scope)
        {
            _fixedScopes.Add(scope);
            ++_fixedCount;
        }

        public void PushScope(IRuntimeValueHolder scope)
        {
            _dynamicScopes.Add(scope);
        }

        public void PopScope()
        {
            if (_dynamicScopes.Count > 0)
                _dynamicScopes.RemoveAt(_dynamicScopes.Count - 1);
        }

        public IRuntimeValueHolder this[int index]
        {
            get
            {
                if (index < _fixedCount)
                    return _fixedScopes[index];
                else
                    return _dynamicScopes[index-_fixedCount];
            }
        }

        public int ScopeCount
        {
            get { return FixedScopeCount + _dynamicScopes.Count; }
        }

        public int FixedScopeCount
        {
            get { return _fixedCount; }
        }
    }
}
