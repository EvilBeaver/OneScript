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
        private MapImpl _files = new MapImpl();

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
                    _files.Insert(
                        ValueFactory.Create(file.Name),
                        ValueFactory.Create(new PostFileDescription(file))
                    );
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
                    IValue key = ValueFactory.Create(Decode(nameVal[0]));
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
            // https://msdn.microsoft.com/ru-ru/library/system.uri.unescapedatastring(v=vs.110).aspx
            // UnescapeDataString не преобразовывает "+" в пробелы
            return System.Uri.UnescapeDataString(p.Replace("+", "%20"));
        }

        /// <summary>
        /// Параметры запроса
        /// </summary>
        [ContextProperty("Параметры", "Params")]
        public MapImpl Params
        { get { return _params; } }

        /// <summary>
        /// Загруженные файлы
        /// </summary>
        [ContextProperty("Файлы", "Files")]
        public MapImpl Files
        { get { return _files; } }

        [ScriptConstructor(Name="Из двоичных данных")]
        public static PostRequestData Constructor(BinaryDataContext data, IValue boundary)
        {
            return new PostRequestData(data.Buffer, boundary.ToString());
        }

        [ScriptConstructor(Name="Из строки запроса")]
        public static PostRequestData Constructor(IValue RequestString)
        {
            return new PostRequestData(RequestString.ToString());
        }
    }
}
