/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HttpMultipartParser;
using ScriptEngine.HostedScript.Library.Http.Multipart;

namespace oscript
{
    [ContextClass("ВебЗапрос", "WebRequest")]
    public class WebRequestContext : AutoContext<WebRequestContext>
    {
        MapImpl _environmentVars = new MapImpl();
        PostRequestData _post;
        byte[] _post_raw = null;

        public WebRequestContext()
        {
            string get = Environment.GetEnvironmentVariable("QUERY_STRING");
            if (get != null)
            {
                FillGetMap(get);
            }

            ProcessPostData();

            FillEnvironmentVars();

        }

        private void ProcessPostData()
        {
            var contentLen = Environment.GetEnvironmentVariable("CONTENT_LENGTH");
            if (contentLen == null)
                return;

            int len = Int32.Parse(contentLen);
            if (len == 0)
                return;

            // THINK: правильно ли хранить всегда весь запрос в памяти???
            _post_raw = new byte[len];
            using (var stdin = Console.OpenStandardInput())
            {
                stdin.Read(_post_raw, 0, len);
            }

            var type = Environment.GetEnvironmentVariable("CONTENT_TYPE");
            if (type != null && type.StartsWith("multipart/"))
            {
                var boundary = type.Substring(type.IndexOf('=') + 1);
                _post = new PostRequestData(_post_raw, boundary);
            }
            else
            {
                _post = new PostRequestData(Encoding.UTF8.GetString(_post_raw));
            }

        }

        private void FillEnvironmentVars()
        {
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                _environmentVars.Insert(
                    ValueFactory.Create((string)item.Key),
                    ValueFactory.Create((string)item.Value));
            }
        }

        private void FillGetMap(string get)
        {
            _post = new PostRequestData(get);
        }

        // TODO: пометить Deprecated
        [ContextProperty("GET")]
        public IValue GET
        {
            get
            {
                return _post.Params;
            }
        }

        // TODO: пометить Deprecated
        [ContextProperty("POST")]
        public IValue POST
        {
            get
            {
                return _post;
            }
        }

        [ContextProperty("Параметры", "Params")]
        public MapImpl Params
        {
            get
            {
                return _post.Params;
            }
        }

        [ContextProperty("Файлы", "Files")]
        public ArrayImpl Files
        {
            get
            {
                return _post.Files;
            }
        }

        [ContextProperty("ENV")]
        public IValue ENV
        {
            get
            {
                return _environmentVars;
            }
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public BinaryDataContext GetBodyAsBinaryData()
        {
            return new BinaryDataContext(_post_raw);
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public string GetBodyAsString(IValue encoding = null)
        {
            Encoding enc = (encoding == null || ValueFactory.Create().Equals(encoding))
                    ? new UTF8Encoding(false)
                    : TextEncodingEnum.GetEncoding(encoding);

            return enc.GetString(_post_raw);
        }
    }
}
