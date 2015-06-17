/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using System;
using System.IO;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyFile : IHttpRequestBody
    {
        private FileStream _bodyOpenedFile;

        public HttpRequestBodyFile(string filename)
        {
            _bodyOpenedFile = new FileStream(filename, FileMode.Open);
        }

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
            return ValueFactory.Create(_bodyOpenedFile.Name);
        }

        public Stream GetDataStream()
        {
            return _bodyOpenedFile;
        }

        public void Dispose()
        {
            _bodyOpenedFile.Dispose();
        }
    }
}