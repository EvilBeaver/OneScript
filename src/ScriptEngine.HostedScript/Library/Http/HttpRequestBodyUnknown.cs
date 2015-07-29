/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyUnknown : IHttpRequestBody
    {

        public IValue GetAsString()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsBinary()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public void Dispose()
        {
            ;
        }

        public System.IO.Stream GetDataStream()
        {
            return null;
        }
    }
}