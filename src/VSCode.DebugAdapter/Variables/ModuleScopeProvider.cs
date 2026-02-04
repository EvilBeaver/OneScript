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
    /// Провайдер переменных модуля
    /// </summary>
    public class ModuleScopeProvider : IVariablesProvider
    {
        private readonly int _threadId;
        private readonly int _frameIndex;

        public ModuleScopeProvider(int threadId, int frameIndex)
        {
            _threadId = threadId;
            _frameIndex = frameIndex;
        }

        public Variable[] FetchVariables(IDebuggerService service)
        {
            // path пустой - получаем переменные модуля верхнего уровня фрейма
            return service.GetModuleVariables(_threadId, _frameIndex, Array.Empty<int>());
        }

        public IVariablesProvider CreateChildProvider(int variableIndex)
        {
            return new ChildVariablesProvider(
                _threadId, 
                _frameIndex, 
                new[] { variableIndex },
                (service, tid, fid, path) => service.GetModuleVariables(tid, fid, path));
        }
    }
}

