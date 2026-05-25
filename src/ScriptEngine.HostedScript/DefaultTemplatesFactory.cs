/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary.Binary;

namespace ScriptEngine.HostedScript
{
    public class DefaultTemplatesFactory : ITemplateFactory
    {
        private readonly int _inMemLimit;
        
        public DefaultTemplatesFactory(IBinaryDataMemoryLimit memoryLimit)
        {
            _inMemLimit = memoryLimit.MaxBytesInMemory;
        }

        public ITemplate CreateTemplate(string file, TemplateKind kind)
        {
            return new FileSourceTemplate(_inMemLimit, file, kind);
        }
    }
}