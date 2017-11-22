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

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library.Http
{
    /// <summary>
    /// Данные и заголоки HTTP запроса.
    /// </summary>
//Свойства:
//
//АдресРесурса(ResourceAddress)
//Заголовки(Headers)
//
//Методы:

//+ПолучитьИмяФайлаТела(GetBodyFileName)
//+ПолучитьТелоКакДвоичныеДанные(GetBodyAsBinary)
//+ПолучитьТелоКакПоток(GetBodyAsStream)
//+ПолучитьТелоКакСтроку(GetBodyAsString)
//+УстановитьИмяФайлаТела(SetBodyFileName)
//+УстановитьТелоИзДвоичныхДанных(SetBodyFromBinary)
//+УстановитьТелоИзСтроки(SetBodyFromString)
//
//Конструкторы:

//По адресу ресурса и заголовкам
//Формирование неинициализированного объекта


    [ContextClass("HTTPЗапрос", "HTTPRequest")]
    public class HttpRequestContext : AutoContext<HttpRequestContext>
    {
        System.IO.Stream _bodyStream;

        public HttpRequestContext()
        {
            ResourceAddress = "";
            Headers = new MapImpl();
            _bodyStream = null;
        }

        public Stream BodyStream
        {
            get
            {
                return _bodyStream;
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
        public void SetBodyFileName (string filename)
        {
            _bodyStream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        }

        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName ()
        {
            if ((_bodyStream == null) || (_bodyStream as System.IO.FileStream) == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(((System.IO.FileStream)_bodyStream).Name);
        }

        /// <summary>
        /// Установить тело запроса из объекта ДвоичныеДанные
        /// </summary>
        /// <param name="data"></param>
        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinary")]
        public void SetBodyFromBinary (BinaryDataContext data)
        {
            _bodyStream = new System.IO.MemoryStream();
            _bodyStream.Write(data.Buffer, 0, data.Buffer.Length);
            _bodyStream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinary")]
        public IValue GetBodyFromBinary()
        {
            if ((_bodyStream == null) || (_bodyStream as System.IO.MemoryStream) == null)
                return null;
            else
                return new BinaryDataContext(((System.IO.MemoryStream)_bodyStream).GetBuffer());
        }

        /// <summary>
        /// Установить строку в качестве содержимого запроса
        /// </summary>
        /// <param name="data">Строка с данными</param>
        /// <param name="encoding">КодировкаТекста или Строка. Кодировка в которой отправляются данные.</param>
        /// <param name="useBOM">Использовать метку порядка байтов (BOM) для кодировок. Тип не реазизован. Параметр добавлен для совместимости и не используется.</param>
        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string data, IValue encoding = null, IValue useBOM = null)
        {
            System.Text.Encoding enc = System.Text.Encoding.UTF8;

            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);

            _bodyStream = new System.IO.MemoryStream();
            byte[] buffer = enc.GetBytes(data);
            _bodyStream.Write(buffer, 0, buffer.Length);
            _bodyStream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            if ((_bodyStream == null) || (_bodyStream as System.IO.MemoryStream) == null)
                return ValueFactory.Create();
            
            // Проверено экспериментально, возвращается строка в UTF8
            return ValueFactory.Create(System.Text.Encoding.UTF8.GetString( ((System.IO.MemoryStream)_bodyStream).GetBuffer() ));
        }

        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public GenericStream GetBodyAsStream()
        {
            if (_bodyStream == null)
                _bodyStream = new System.IO.MemoryStream();

            return new GenericStream(_bodyStream);
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
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
