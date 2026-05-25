/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Text;
using OneScript.Types;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.Http
{
    class HttpRequestBodyBinary : IHttpRequestBody
    {
        private readonly FileBackingStream _storage;

        internal HttpRequestBodyBinary(int inMemoryBodyLimit)
        {
            _storage = new FileBackingStream(inMemoryBodyLimit);
        }

        internal HttpRequestBodyBinary(int inMemoryBodyLimit, BinaryDataContext data)
        {
            _storage = new FileBackingStream(inMemoryBodyLimit);
            data.CopyTo(_storage);
        }

        public IValue GetAsString()
        {
            _storage.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(_storage);
            return ValueFactory.Create(reader.ReadToEnd());
        }

        public IValue GetAsBinary()
        {
            _storage.Seek(0, SeekOrigin.Begin);
            return new BinaryDataContext(_storage, _storage.InMemoryThreshold);
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public Stream GetDataStream()
        {
            _storage.Seek(0, SeekOrigin.Begin);
            return _storage;
        }

        public void Dispose()
        {
            _storage.Close();
        }
    }
}