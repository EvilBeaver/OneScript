/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

using ScriptEngine.HostedScript;

namespace StandaloneRunner
{
    internal class StandaloneTemplateFactory : ITemplateFactory
    {
        public ITemplate CreateTemplate(string file, TemplateKind kind)
        {
            throw new NotImplementedException();
        }
    }
}