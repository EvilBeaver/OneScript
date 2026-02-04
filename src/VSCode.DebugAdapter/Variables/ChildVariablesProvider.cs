/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.DebugProtocol;

namespace VSCode.DebugAdapter
{
    /// <summary>
    /// Универсальный провайдер вложенных переменных
    /// </summary>
    public class ChildVariablesProvider : IVariablesProvider
    {
        private readonly int _threadId;
        private readonly int _frameIndex;
        private readonly int[] _path;
        private readonly Func<IDebuggerService, int, int, int[], Variable[]> _fetchFunc;

        public ChildVariablesProvider(
            int threadId, 
            int frameIndex, 
            int[] path,
            Func<IDebuggerService, int, int, int[], Variable[]> fetchFunc)
        {
            _threadId = threadId;
            _frameIndex = frameIndex;
            _path = path;
            _fetchFunc = fetchFunc;
        }

        public Variable[] FetchVariables(IDebuggerService service)
        {
            return _fetchFunc(service, _threadId, _frameIndex, _path);
        }

        public IVariablesProvider CreateChildProvider(int variableIndex)
        {
            var newPath = new int[_path.Length + 1];
            Array.Copy(_path, newPath, _path.Length);
            newPath[_path.Length] = variableIndex;
            return new ChildVariablesProvider(_threadId, _frameIndex, newPath, _fetchFunc);
        }
    }
}

