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
    /// Провайдер переменных для вычисленного выражения
    /// </summary>
    public class EvaluatedExpressionProvider : IVariablesProvider
    {
        private readonly string _expression;
        private readonly int _threadId;
        private readonly int _frameIndex;
        private readonly int[] _path;

        public EvaluatedExpressionProvider(string expression, int threadId, int frameIndex)
        {
            _expression = expression;
            _threadId = threadId;
            _frameIndex = frameIndex;
            _path = Array.Empty<int>();
        }

        private EvaluatedExpressionProvider(string expression, int threadId, int frameIndex, int[] path)
        {
            _expression = expression;
            _threadId = threadId;
            _frameIndex = frameIndex;
            _path = path;
        }

        public Variable[] FetchVariables(IDebuggerService service)
        {
            return service.GetEvaluatedVariables(_expression, _threadId, _frameIndex, _path);
        }

        public IVariablesProvider CreateChildProvider(int variableIndex)
        {
            var newPath = new int[_path.Length + 1];
            Array.Copy(_path, newPath, _path.Length);
            newPath[_path.Length] = variableIndex;
            return new ChildVariablesProvider(
                _threadId, 
                _frameIndex, 
                newPath,
                (service, tid, fid, path) => service.GetEvaluatedVariables(_expression, tid, fid, path));
        }
    }
}

