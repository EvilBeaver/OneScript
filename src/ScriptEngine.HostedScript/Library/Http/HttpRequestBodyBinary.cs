/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using System;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyBinary : IHttpRequestBody
    {
        BinaryDataContext _data;

        public HttpRequestBodyBinary(BinaryDataContext data)
        {
            _data = data;
        }

        public IValue GetAsString()
        {
            return ValueFactory.Create();
        }

        public IValue GetAsBinary()
        {
            return _data;
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public System.IO.Stream GetDataStream()
        {
            var bytes = _data.Buffer;
            return new System.IO.MemoryStream(bytes);
        }

        public void Dispose()
        {
            _data = null;
        }
    }
}