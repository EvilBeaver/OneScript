using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Json;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Text;

namespace OneScript.StandardLibrary.Http.Web
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
        public void Write(IValue strData, IValue encoding = null)
        {
            var enc = encoding == null ? Encoding.UTF8 : TextEncodingEnum.GetEncoding(encoding);

            _response.WriteAsync(strData.AsString(), enc).Wait();
        }

        [ContextMethod("ЗаписатьКакJson", "WriteAsJson")]
        public void WriteJson(IValue obj)
        {
            var writer = new JSONWriter();
            writer.SetString();

            var jsonFunctions = GlobalJsonFunctions.CreateInstance() as GlobalJsonFunctions;
            jsonFunctions.WriteJSON(writer, obj);

            var data = writer.Close();

            _response.ContentType = $"application/json;charset={Encoding.UTF8.WebName}";
            _response.WriteAsync(data, Encoding.UTF8).Wait();
        }
    }
}
