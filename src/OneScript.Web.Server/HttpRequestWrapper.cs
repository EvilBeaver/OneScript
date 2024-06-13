using Microsoft.AspNetCore.Http;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Binary;
using OneScript.Web.Server;
using OneScript.StandardLibrary.Http;
using OneScript.StandardLibrary.Processes;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Text;
using System.Threading.Tasks;
using OneScript.StandardLibrary.Collections;

namespace OneScript.Web.Server
{
    [ContextClass("HTTPСервисЗапрос", "HTTPServiceRequest")]
    public class HttpRequestWrapper : AutoContext<HttpRequestWrapper>
    {
        private readonly HttpRequest _request;

        public HttpRequestWrapper(HttpRequest request)
        {
            _request = request;
        }

        [ContextProperty("Параметры", "Parameters", CanWrite = false)]
        public FixedMapImpl Query => _request.Query.ToFixedMap();

        [ContextProperty("ЕстьФормыВТипеКонтента", "HasFormContentType", CanWrite = false)]
        public IValue HasFormContentType => BslBooleanValue.Create(_request.HasFormContentType);

        [ContextProperty("Тело", "Body", CanWrite = false)]
        public GenericStream Body => new GenericStream(_request.Body);

        [ContextProperty("ТипКонтента", "ContentType", CanWrite = false)]
        public IValue ContentType
        {
            get
            {
                if (_request.ContentType == null)
                    return BslNullValue.Instance;
                else
                    return BslStringValue.Create(_request.ContentType);
            }
        }

        [ContextProperty("ДлинаКонтента", "ContentLength", CanWrite = false)]
        public IValue ContentLength
        {
            get
            {
                if (_request.ContentLength == null)
                    return BslNullValue.Instance;
                else
                    return BslNumericValue.Create((decimal)_request.ContentLength);
            }
        }

        [ContextProperty("Куки", "Cookie", CanWrite = false)]
        public RequestCookieCollectionWrapper Cookies => new RequestCookieCollectionWrapper(_request.Cookies);

        [ContextProperty("Заголовки", "Headers", CanWrite = false)]
        public HeaderDictionaryWrapper Headers => new HeaderDictionaryWrapper(_request.Headers);

        [ContextProperty("Протокол", "Protocol", CanWrite = false)]
        public IValue Protocol => BslStringValue.Create(_request.Protocol);

        [ContextProperty("СтрокаПараметров", "ParametersString", CanWrite = false)]
        public IValue QueryString
        {
            get
            {
                if (_request.QueryString.HasValue)
                    return BslStringValue.Create(_request.QueryString.Value);
                else
                    return BslNullValue.Instance;
            }
        }

        [ContextProperty("Путь", "Path", CanWrite = false)]
        public IValue Path
        {
            get
            {
                if (_request.Path.HasValue)
                    return BslStringValue.Create(_request.Path.Value);
                else
                    return BslNullValue.Instance;
            }
        }

        [ContextProperty("БазовыйПуть", "PathBase", CanWrite = false)]
        public IValue PathBase 
        {
            get
            {
                if (_request.PathBase.HasValue)
                    return BslStringValue.Create(_request.PathBase);
                else
                    return BslNullValue.Instance;
            }
        }

        [ContextProperty("Хост", "Host", CanWrite = false)]
        public IValue Host
        {
            get
            {
                if (_request.Host.HasValue)
                    return BslStringValue.Create(_request.Host.Value);
                else
                    return BslNullValue.Instance;
            }
        }

        [ContextProperty("ЭтоHttps", "IsHttps", CanWrite = false)]
        public IValue IsHttps => BslBooleanValue.Create(_request.IsHttps);

        [ContextProperty("Схема", "Scheme", CanWrite = false)]
        public IValue Scheme => BslStringValue.Create(_request.Scheme);

        [ContextProperty("Метод", "Method", CanWrite = false)]
        public IValue Method => BslStringValue.Create(_request.Method);
    }
}
