/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Http
{
    class HttpRequestBodyBinary : IHttpRequestBody
    {
        private readonly MemoryStream _memoryStream = new MemoryStream();

        public HttpRequestBodyBinary()
        {
        }

        public HttpRequestBodyBinary(BinaryDataContext data)
        {
            _memoryStream.Write(data.Buffer, 0, data.Size());
        }

        public HttpRequestBodyBinary(string body, IValue encoding = null,
            ByteOrderMarkUsageEnum bomUsage = ByteOrderMarkUsageEnum.Auto)
        {
            var utfs = new List<string> {"utf-16", "utf-32"};
            var addBom = utfs.Contains(encoding?.AsString(), StringComparer.OrdinalIgnoreCase) &&
                         bomUsage == ByteOrderMarkUsageEnum.Auto || bomUsage == ByteOrderMarkUsageEnum.Use;

            var encoder = encoding == null ? new UTF8Encoding(addBom) : TextEncodingEnum.GetEncoding(encoding, addBom);

            var byteArray = encoder.GetBytes(body);
            _memoryStream.Write(byteArray, 0, byteArray.Length);
        }

        public IValue GetAsString()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(_memoryStream);
            return ValueFactory.Create(reader.ReadToEnd());
        }

        public IValue GetAsBinary()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            return new BinaryDataContext(_memoryStream.ToArray());
        }

        public IValue GetAsFilename()
        {
            return ValueFactory.Create();
        }

        public Stream GetDataStream()
        {
            _memoryStream.Seek(0, SeekOrigin.Begin);
            return _memoryStream;
        }

        public void Dispose()
        {
            _memoryStream.Close();
        }
    }
}