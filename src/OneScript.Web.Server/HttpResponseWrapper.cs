﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.Web.Server;
using OneScript.StandardLibrary.Json;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Text;

namespace OneScript.Web.Server
{

    [ContextClass("HTTPСервисОтвет", "HTTPServiceResponse")]
    public class HttpResponseWrapper: AutoContext<HttpResponseWrapper>
    {
        private readonly HttpResponse _response;

        public HttpResponseWrapper(HttpResponse response)
        {
            _response = response;
        }

        [ContextProperty("Начат", "HasStarted", CanWrite = false)]
        public IValue HasStarted => BslBooleanValue.Create(_response.HasStarted);

        [ContextProperty("ТипКонтента", "ContentType")]
        public IValue ContentType
        {
            get => BslStringValue.Create(_response.ContentType);
            set => _response.ContentType = value.AsString();
        }

        [ContextProperty("ДлинаКонтента", "ContentLength")]
        public IValue ContentLength
        {
            get
            {
                if (_response.ContentLength == null)
                    return BslNullValue.Instance;
                else
                    return BslNumericValue.Create((decimal)_response.ContentLength);
            }
            set
            {
                if (value == null)
                    _response.ContentLength = null;
                else
                    _response.ContentLength = (long)value.AsNumber();
            }
        }

        [ContextProperty("Тело", "Body", CanWrite = false)]
        public GenericStream Body => new GenericStream(_response.Body);

        [ContextProperty("Заголовки", "Headers", CanWrite = false)]
        public HeaderDictionaryWrapper Headers => new HeaderDictionaryWrapper(_response.Headers);

        [ContextProperty("КодСостояния", "StatusCode")]
        public IValue StatusCode
        {
            get => BslNumericValue.Create(_response.StatusCode);
            set
            {
                _response.StatusCode = (int)value.AsNumber();
            }
        }

        [ContextProperty("Куки", "Cookie", CanWrite = false)]
        public ResponseCookiesWrapper Cookies => new ResponseCookiesWrapper(_response.Cookies);

        [ContextMethod("Записать", "Write")]
        public void Write(string strData, IValue encoding = null)
        {
            var enc = encoding == null ? Encoding.UTF8 : TextEncodingEnum.GetEncoding(encoding);

            _response.ContentLength = enc.GetByteCount(strData);
            _response.WriteAsync(strData, enc).Wait();
        }

        [ContextMethod("ЗаписатьКакJson", "WriteAsJson")]
        public void WriteJson(IValue obj, IValue encoding = null)
        {
            var enc = encoding == null ? Encoding.UTF8 : TextEncodingEnum.GetEncoding(encoding);

            var writer = new JSONWriter();
            writer.SetString();

            var jsonFunctions = GlobalJsonFunctions.CreateInstance() as GlobalJsonFunctions;
            jsonFunctions.WriteJSON(writer, obj);

            var data = writer.Close();

            _response.ContentType = $"application/json;charset={enc.WebName}";
            _response.ContentLength = enc.GetByteCount(data);
            _response.WriteAsync(data, enc).Wait();
        }
    }
}
