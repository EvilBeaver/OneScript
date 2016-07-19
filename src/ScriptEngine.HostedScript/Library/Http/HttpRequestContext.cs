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
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.Http
{
    /// <summary>
    /// Данные и заголоки HTTP запроса.
    /// </summary>
    [ContextClass("HTTPЗапрос", "HTTPRequest")]
    public class HttpRequestContext : AutoContext<HttpRequestContext>
    {

        IHttpRequestBody _body;
        static readonly IHttpRequestBody _emptyBody = new HttpRequestBodyUnknown();

        public HttpRequestContext()
        {
            ResourceAddress = "";
            Headers = new MapImpl();
            _body = _emptyBody;
        }

        public void Close()
        {
            SetBody(_emptyBody);
        }

        private void SetBody(IHttpRequestBody newBody)
        {
            _body.Dispose();
            _body = newBody;
        }

        public Stream Body
        {
            get
            {
                return _body.GetDataStream();
            }
        }

        /// <summary>
        /// Относительный путь к ресурсу на сервере (не включает имя сервера)
        /// </summary>
        [ContextProperty("АдресРесурса", "ResourceAddress")]
        public string ResourceAddress { get; set; }

        /// <summary>
        /// Соответствие. Заголовки запроса к http-серверу
        /// </summary>
        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers { get; set; }

        /// <summary>
        /// Установить файл на диске в качестве тела запроса. Файл открывается на чтение и остается открытым до завершения запроса.
        /// </summary>
        /// <param name="filename"></param>
        [ContextMethod("УстановитьИмяФайлаТела", "SetBodyFileName")]
        public void SetBodyFileName(string filename)
        {
            SetBody(new HttpRequestBodyFile(filename));
        }

        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            return _body.GetAsFilename();
        }

        /// <summary>
        /// Установить тело запроса из объекта ДвоичныеДанные
        /// </summary>
        /// <param name="data"></param>
        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinary")]
        public void SetBodyFromBinary(BinaryDataContext data)
        {
            SetBody(new HttpRequestBodyBinary(data));
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinary")]
        public IValue GetBodyFromBinary()
        {
            return _body.GetAsBinary();
        }

        /// <summary>
        /// Установить строку в качестве содержимого запроса
        /// </summary>
        /// <param name="data">Строка с данными</param>
        /// <param name="encoding">КодировкаТекста или Строка. Кодировка в которой отправляются данные.</param>
        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string data, IValue encoding = null)
        {
            SetBody(new HttpRequestBodyString(data, encoding));
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            return _body.GetAsString();
        }

        [ScriptConstructor]
        public static HttpRequestContext Constructor()
        {
            return new HttpRequestContext();
        }

        [ScriptConstructor(Name = "По адресу ресурса и заголовкам")]
        public static HttpRequestContext Constructor(IValue resource, IValue headers = null)
        {
            var ctx = new HttpRequestContext();
            ctx.ResourceAddress = resource.AsString();
            if (headers != null)
            {
                var headersMap = headers.GetRawValue() as MapImpl;
                if (headersMap == null)
                    throw RuntimeException.InvalidArgumentType();

                ctx.Headers = headersMap;
            }

            return ctx;
        }

    }
}
