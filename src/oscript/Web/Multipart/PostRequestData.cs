/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using HttpMultipartParser;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.IO;
using System.Text;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;

namespace oscript.Web.Multipart
{
    [ContextClass("ДанныеPOSTЗапроса", "PostRequestData")]
    public class PostRequestData : AutoContext<PostRequestData>
    {

        private FixedMapImpl _params;
        private FixedMapImpl _files;

        public PostRequestData(byte []buffer, string boundary)
        {
            using (var stdin = new MemoryStream(buffer))
            {
                var parser = new MultipartFormDataParser(stdin, boundary, Encoding.UTF8);
                MapImpl m_params = new MapImpl();
                foreach (var param in parser.Parameters)
                {
                    m_params.Insert(ValueFactory.Create(param.Name), ValueFactory.Create(param.Data));
                }
                _params = new FixedMapImpl(m_params);

                MapImpl m_files = new MapImpl();
                foreach (var file in parser.Files)
                {
                    m_files.Insert(
                        ValueFactory.Create(file.Name),
                        new PostFileDescription(file)
                    );
                }
                _files = new FixedMapImpl(m_files);
            }
        }

        public PostRequestData(string data)
        {
            MapImpl m_params = new MapImpl();

            var pairs = data.Split('&');
            foreach (var pair in pairs)
            {
                var nameVal = pair.Split(new Char[] { '=' }, 2);
                if (nameVal.Length == 2)
                {
                    IValue key = ValueFactory.Create(Decode(nameVal[0]));
                    IValue val = ValueFactory.Create(Decode(nameVal[1]));
                    m_params.Insert(key, val);
                }
                else if (pair.Length > 0)
                {
                    IValue val = ValueFactory.Create(Decode(pair));
                    m_params.Insert(val, ValueFactory.Create());
                }

                _params = new FixedMapImpl(m_params);
                _files = new FixedMapImpl(new MapImpl());
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
        public FixedMapImpl Params
        { get { return _params; } }

        /// <summary>
        /// Загруженные файлы
        /// </summary>
        [ContextProperty("Файлы", "Files")]
        public FixedMapImpl Files
        { get { return _files; } }

        [ScriptConstructor(Name = "Из двоичных данных")]
        public static PostRequestData Constructor(BinaryDataContext data, IValue boundary)
        {
            return new PostRequestData(data.Buffer, boundary.ToString());
        }

        [ScriptConstructor(Name = "Из строки запроса")]
        public static PostRequestData Constructor(IValue RequestString)
        {
            return new PostRequestData(RequestString.ToString());
        }
    }
}
