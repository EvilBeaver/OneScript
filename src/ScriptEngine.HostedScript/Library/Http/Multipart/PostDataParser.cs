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
    class PostRequestData : AutoContext<PostRequestData>
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
