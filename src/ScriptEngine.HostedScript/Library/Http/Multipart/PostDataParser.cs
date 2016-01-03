/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HttpMultipartParser;

namespace ScriptEngine.HostedScript.Library.Http.Multipart
{
    [ContextClass("ДанныеPOSTЗапроса", "PostRequestData")]
    public class PostRequestData : AutoContext<PostRequestData>
    {

        private MapImpl _params = new MapImpl();
        private ArrayImpl _files = new ArrayImpl(); // TODO: MapImpl для файлов POST-запроса

        public PostRequestData(byte []buffer, string boundary)
        {
            using (var stdin = new MemoryStream(buffer))
            {
                var parser = new MultipartFormDataParser(stdin, boundary, Encoding.UTF8);
                foreach (var param in parser.Parameters)
                {
                    _params.Insert(ValueFactory.Create(param.Name), ValueFactory.Create(param.Data));
                }

                foreach (var file in parser.Files)
                {
                    _files.Add(ValueFactory.Create(new PostFileDescription(file)));
                }
            }
        }

        public PostRequestData(string data)
        {
            var pairs = data.Split('&');
            foreach (var pair in pairs)
            {
                var nameVal = pair.Split(new Char[] { '=' }, 2);
                if (nameVal.Length == 2)
                {
                    IValue key = ValueFactory.Create(nameVal[0]);
                    IValue val = ValueFactory.Create(Decode(nameVal[1]));
                    _params.Insert(key, val);
                }
                else if (pair.Length > 0)
                {
                    IValue val = ValueFactory.Create(Decode(pair));
                    _params.Insert(val, ValueFactory.Create());
                }
            }
        }

        private static string Decode(string p)
        {
            byte[] bytes = new byte[p.Length];

            int j = 0;
            for (int i = 0; i < p.Length; i++, j++)
            {
                if (p[i] == '+')
                    bytes[j] = 0x20;
                else if (p[i] == '%')
                {
                    bytes[j] = ByteFromHEX(p, ref i);
                }
                else
                    bytes[j] = (byte)p[i];
            }

            return Encoding.UTF8.GetString(bytes, 0, j);

        }

        private static byte ByteFromHEX(string pattern, ref int index)
        {
            System.Diagnostics.Debug.Assert(pattern[index] == '%');

            index++;
            int major = 0;
            int minor = 0;
            for (int i = 0; i < 2; i++)
            {
                int code = (int)pattern[index];
                if (code >= 48 && code <= 57)
                    code = code - 48;
                else if (code >= 65 && code <= 70)
                    code = code - 55;

                if (i == 0)
                {
                    major = code;
                    index++;
                }
                else
                    minor = code;
            }

            return (byte)(major * 16 + minor);

        }

        [ContextProperty("Параметры", "Params")]
        public MapImpl Params
        { get { return _params; } }

        [ContextProperty("Файлы", "Files")]
        public ArrayImpl Files
        { get { return _files; } }

        [ScriptConstructor(Name="Из двоичных данных")]
        public static PostRequestData Constructor(BinaryDataContext data, IValue boundary)
        {
            return new PostRequestData(data.Buffer, boundary.ToString());
        }
    }
}
