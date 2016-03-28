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
using HttpMultipartParser;

namespace ScriptEngine.HostedScript.Library.Http.Multipart
{
    [ContextClass("ОписаниеФайлаPostЗапроса", "PostFileDecription")]
    class PostFileDescription : AutoContext<PostFileDescription>
    {
        private string _name;
        private string _filename;
        private string _content_disposition;
        private string _content_type;
        private long _content_size;
        private BinaryDataContext _data;

        internal PostFileDescription(FilePart filepart)
        {
            _name = filepart.Name;
            _filename = filepart.FileName;
            _content_disposition = filepart.ContentDisposition;
            _content_type = filepart.ContentType;
            _content_size = filepart.Data.Length;

            byte[] buf = new byte[_content_size];
            filepart.Data.Read(buf, 0, (int)_content_size);

            _data = new BinaryDataContext(buf);
        }

        [ContextProperty("Имя", "Name")]
        public string Name
        { get { return _name; } }

        [ContextProperty("ИмяФайла", "FileName")]
        public string Filename
        { get { return _filename; } }

        [ContextProperty("РасположениеСодержимого", "ContentDisposition")]
        public string ContentDisposition
        { get { return _content_disposition; } }

        [ContextProperty("ТипСодержимого", "ContentType")]
        public string ContentType
        { get { return _content_type; } }

        [ContextProperty("Размер", "Size")]
        public decimal ContentSize
        { get { return _content_size; } }

        [ContextProperty("Данные", "Data")]
        public BinaryDataContext Data
        { get { return _data; } }

    }
}
