/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Core;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Http
{
    /// <summary>
    /// Данные и заголоки HTTP запроса.
    /// </summary>
    [ContextClass("HTTPЗапрос", "HTTPRequest")]
    public class HttpRequestContext : AutoContext<HttpRequestContext>, IDisposable
    {
        private IHttpRequestBody _body;

        private HttpRequestContext()
        {
            ResourceAddress = "";
            Headers = new MapImpl();
        }

        public void Close()
        {
            SetBody(null);
        }

        private void SetBody(IHttpRequestBody newBody)
        {
            _body?.Dispose();
            _body = newBody;
        }

        public Stream Body => _body?.GetDataStream();

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
            return _body?.GetAsFilename();
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
            return _body?.GetAsBinary();
        }

        /// <summary>
        /// Установить строку в качестве содержимого запроса
        /// </summary>
        /// <param name="data">Строка с данными</param>
        /// <param name="encoding">КодировкаТекста или Строка. Кодировка в которой отправляются данные.</param>
        /// <param name="bomUsage">Использовать метку порядка байтов (BOM) для кодировок, которые ее поддерживают.</param>
        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string data, IValue encoding = null, ByteOrderMarkUsageEnum bomUsage = ByteOrderMarkUsageEnum.Auto)
        {
            SetBody(new HttpRequestBodyBinary(data, encoding, bomUsage));
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            return _body?.GetAsString();
        }

        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public GenericStream GetBodyAsStream()
        {
            _body = _body ?? new HttpRequestBodyBinary();
            return new GenericStream(_body.GetDataStream());
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static HttpRequestContext Constructor()
        {
            return new HttpRequestContext();
        }

        [ScriptConstructor(Name = "По адресу ресурса и заголовкам")]
        public static HttpRequestContext Constructor(IValue resource, IValue headers = null)
        {
            var ctx = new HttpRequestContext {ResourceAddress = resource.AsString()};
            if (headers == null) return ctx;
            if (!(headers.GetRawValue() is MapImpl headersMap))
                throw RuntimeException.InvalidArgumentType();

            ctx.Headers = headersMap;

            return ctx;
        }

        public void Dispose()
        {
            _body?.Dispose();
        }
    }
}
