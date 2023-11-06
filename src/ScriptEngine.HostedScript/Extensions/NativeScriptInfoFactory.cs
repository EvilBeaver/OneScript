/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Extensions
{
    public class NativeScriptInfoFactory : IScriptInformationFactory
    {
        public BslObjectValue GetInfo(SourceCode source)
        {
            return new ScriptInformationContext(source);
        }
    }
}