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
using System.Net;
using System.Text;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Http
{
    /// <summary>
    /// Ответ от HTTP-сервера
    /// </summary>
    /// 

//Свойства:
//
//Заголовки(Headers)
//КодСостояния(StatusCode)
//
//Методы:
//
//ПолучитьИмяФайлаТела(GetBodyFileName)
//ПолучитьТелоКакДвоичныеДанные(GetBodyAsBinaryData)
//ПолучитьТелоКакПоток(GetBodyAsStream)
//ПолучитьТелоКакСтроку(GetBodyAsString)


    [ContextClass("HTTPОтвет", "HTTPResponse")]
    public class HttpResponseContext : AutoContext<HttpResponseContext>, IDisposable
    {
        private readonly MapImpl _headers = new MapImpl();
        // TODO: Нельзя выделить массив размером больше чем 2GB
        // поэтому функционал сохранения в файл не должен использовать промежуточный буфер _body
        // Этот объект создается только из соединения
        // Возможные варианты Загрузка в файл (_bodyStream == null && _filename != null), В память (_bodyStream != null && _fileName == null) 
        System.IO.MemoryStream _bodyStream = null;
        string _fileName = null;

        private string _defaultCharset;

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
            if (dumpToFile != null)
            {
                System.IO.FileStream fs = new System.IO.FileStream(dumpToFile, FileMode.Create, FileAccess.ReadWrite);
                response.GetResponseStream().CopyTo(fs);
                fs.Flush();
                fs.Close();
                _fileName = dumpToFile;
            }
            else
            {
                _bodyStream = new System.IO.MemoryStream();
                response.GetResponseStream().CopyTo(_bodyStream);
                _bodyStream = new System.IO.MemoryStream(_bodyStream.GetBuffer(), false);
            }
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
            if (_bodyStream == null)
                return ValueFactory.Create();

            System.Text.Encoding enc = TextEncodingEnum.GetEncodingByName(_defaultCharset);

            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);

            _bodyStream.Seek(0, SeekOrigin.Begin);
            return ValueFactory.Create(enc.GetString(  _bodyStream.GetBuffer() ));
        }

        /// <summary>
        /// Интерпретировать ответ, как ДвоичныеДанные
        /// </summary>
        /// <returns>ДвоичныеДанные</returns>
        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public IValue GetBodyAsBinaryData()
        {
            if (_bodyStream == null)
                return ValueFactory.Create();

            _bodyStream.Seek(0, SeekOrigin.Begin);
            return new BinaryDataContext( ((System.IO.MemoryStream)_bodyStream).GetBuffer() );
        }

        /// <summary>
        /// Получить файл, в который записан ответ сервера.
        /// </summary>
        /// <returns>Строка. Имя файла с ответом. Если ответ не записывался в файл - возвращает Неопределено.</returns>
        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            if (_fileName == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(_fileName);
         }

        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public IValue GetBodyAsStream()
        {
            if (_fileName == null)
                return new GenericStream(_bodyStream);
            else
                return new GenericStream(new System.IO.FileStream(_fileName, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// Закрыть HTTP ответ и освободить ресурсы
        /// </summary
        // В стандарт не входит, а нужно ли оно?
        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_bodyStream != null)
            {
                _bodyStream.Close();
                _bodyStream.Dispose();
                _bodyStream = null;
            }
        }
    }
}
