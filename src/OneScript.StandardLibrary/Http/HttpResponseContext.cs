/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Net;
using System.Text;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Text;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Http
{
    /// <summary>
    /// Ответ от HTTP-сервера
    /// </summary>
    [ContextClass("HTTPОтвет", "HTTPResponse")]
    public class HttpResponseContext : AutoContext<HttpResponseContext>, IDisposable
    {
        private readonly MapImpl _headers = new MapImpl();
        // TODO: Нельзя выделить массив размером больше чем 2GB
        // поэтому функционал сохранения в файл не должен использовать промежуточный буфер _body
        private HttpResponseBody _body;
        
        private string _defaultCharset;
        private string _filename;

        public HttpResponseContext(HttpWebResponse response)
        {
            RetrieveResponseData(response, null);
        }

        public HttpResponseContext(HttpWebResponse response, string dumpToFile)
        {
            RetrieveResponseData(response, dumpToFile);
        }

        private void RetrieveResponseData(HttpWebResponse response, string dumpToFile)
        {
            using(response)
            {
                StatusCode = (int)response.StatusCode;
                _defaultCharset = response.CharacterSet;

                ProcessHeaders(response.Headers);
                ProcessResponseBody(response, dumpToFile);
                if (_body != null && _body.AutoDecompress)
                {
                    _headers.Delete(ValueFactory.Create("Content-Encoding"));
                    _headers.SetIndexedValue(ValueFactory.Create("Content-Length"), ValueFactory.Create(_body.ContentSize));
                }
            }
        }

        private void ProcessHeaders(WebHeaderCollection webHeaderCollection)
        {
            foreach (var item in webHeaderCollection.AllKeys)
            {
                _headers.Insert(ValueFactory.Create(item), ValueFactory.Create(webHeaderCollection[item]));
            }
        }

        private void ProcessResponseBody(HttpWebResponse response, string dumpToFile)
        {
            if (response.ContentLength == 0)
            {
                _body = null;
                return;
            }
            _filename = dumpToFile;
            _body = new HttpResponseBody(response, dumpToFile);

        }

        /// <summary>
        /// Соответствие. Заголовки ответа сервера.
        /// </summary>
        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers
        {
            get
            {
                return _headers;
            }
        }

        /// <summary>
        /// Код состояния HTTP ответа. Число.
        /// </summary>
        [ContextProperty("КодСостояния", "StatusCode", CanWrite = false)]
        public int StatusCode { get; set; }

        /// <summary>
        /// Получает ответ сервера в виде строки
        /// </summary>
        /// <param name="encoding">КодировкаТекста или Строка. Кодировка полученного текста. По умолчанию принимается кодировка из заголовка Content-Type</param>
        /// <returns></returns>
        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString(IValue encoding = null)
        {
            if (_body == null)
                return ValueFactory.Create();

            Encoding enc;
            if (encoding == null)
            {
                if (String.IsNullOrEmpty(_defaultCharset))
                    _defaultCharset = "utf-8";

                enc = Encoding.GetEncoding(_defaultCharset);
            }
            else
                enc = TextEncodingEnum.GetEncoding(encoding);

            using(var reader = new StreamReader(_body.OpenReadStream(), enc))
            {
                return ValueFactory.Create(reader.ReadToEnd());
            }
            
        }

        /// <summary>
        /// Интерпретировать ответ, как ДвоичныеДанные
        /// </summary>
        /// <returns>ДвоичныеДанные</returns>
        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public IValue GetBodyAsBinaryData()
        {
            if (_body == null)
                return ValueFactory.Create();

            using (var stream = _body.OpenReadStream())
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                return new BinaryDataContext(data);
            }
        }

        /// <summary>
        /// Интерпретировать ответ, как Поток
        /// </summary>
        /// <returns>Поток</returns>
        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public IValue GetBodyAsStream()
        {
            if (_body == null)
                return ValueFactory.Create();

            return new GenericStream(_body.OpenReadStream(), true);
        }

        /// <summary>
        /// Получить файл, в который записан ответ сервера.
        /// </summary>
        /// <returns>Строка. Имя файла с ответом. Если ответ не записывался в файл - возвращает Неопределено.</returns>
        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            if (_filename == null)
                return ValueFactory.Create();

            return ValueFactory.Create(_filename);
        }

        /// <summary>
        /// Закрыть HTTP ответ и освободить ресурсы
        /// </summary>
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_body != null)
                _body.Dispose();
        }
    }
}
