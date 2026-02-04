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
    /// Провайдер локальных переменных фрейма стека
    /// </summary>
    public class LocalScopeProvider : IVariablesProvider
    {
        private readonly int _threadId;
        private readonly int _frameIndex;

        public LocalScopeProvider(int threadId, int frameIndex)
        {
            _threadId = threadId;
            _frameIndex = frameIndex;
        }

        public Variable[] FetchVariables(IDebuggerService service)
        {
            // path пустой - получаем переменные верхнего уровня фрейма
            return service.GetVariables(_threadId, _frameIndex, Array.Empty<int>());
        }

        public IVariablesProvider CreateChildProvider(int variableIndex)
        {
            return new ChildVariablesProvider(
                _threadId, 
                _frameIndex, 
                new[] { variableIndex },
                (service, tid, fid, path) => service.GetVariables(tid, fid, path));
        }
    }
}

