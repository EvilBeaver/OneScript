/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using OneScript.StandardLibrary.Collections;

namespace OneScript.Web.Server
{
    [ContextClass("HTTPСервисЗапрос", "HTTPServiceRequest")]
    public class HttpRequestWrapper : AutoContext<HttpRequestWrapper>
    {
        private readonly HttpRequest _request;
        private readonly PropertyWrappersCollection _wrappers = new ();

        public HttpRequestWrapper(HttpRequest request)
        {
            _request = request;
        }

        [ContextProperty("Параметры", "Parameters", CanWrite = false)]
        public FixedMapImpl Query => _wrappers.Get(nameof(Query), () => _request.Query.ToFixedMap());

        [ContextProperty("ЕстьФормыВТипеКонтента", "HasFormContentType", CanWrite = false)]
        public bool HasFormContentType => _request.HasFormContentType;

        [ContextProperty("Тело", "Body", CanWrite = false)]
        public GenericStream Body => _wrappers.Get(nameof(Body), () => new GenericStream(_request.Body));

        [ContextProperty("ТипКонтента", "ContentType", CanWrite = false)]
        public IValue ContentType
        {
            get
            {
                if (_request.ContentType == null)
                    return BslUndefinedValue.Instance;
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
                    return BslUndefinedValue.Instance;
                else
                    return BslNumericValue.Create((decimal)_request.ContentLength);
            }
        }

        [ContextProperty("Куки", "Cookie", CanWrite = false)]
        public RequestCookieCollectionWrapper Cookies => _wrappers.Get(nameof(Cookies), () => new RequestCookieCollectionWrapper(_request.Cookies));

        [ContextProperty("Заголовки", "Headers", CanWrite = false)]
        public HeaderDictionaryWrapper Headers => _wrappers.Get(nameof(Headers), () => new HeaderDictionaryWrapper(_request.Headers));

        [ContextProperty("Протокол", "Protocol", CanWrite = false)]
        public string Protocol => _request.Protocol;

        [ContextProperty("СтрокаПараметров", "ParametersString", CanWrite = false)]
        public IValue QueryString
        {
            get
            {
                if (_request.QueryString.HasValue)
                    return BslStringValue.Create(_request.QueryString.Value);
                else
                    return BslUndefinedValue.Instance;
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
                    return BslUndefinedValue.Instance;
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
                    return BslUndefinedValue.Instance;
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
                    return BslUndefinedValue.Instance;
            }
        }

        [ContextProperty("ЭтоHttps", "IsHttps", CanWrite = false)]
        public bool IsHttps => _request.IsHttps;

        [ContextProperty("Схема", "Scheme", CanWrite = false)]
        public string Scheme => _request.Scheme;

        [ContextProperty("Метод", "Method", CanWrite = false)]
        public string Method => _request.Method;

        [ContextProperty("Форма", "Form", CanWrite = false)]
        public IValue Form
        {
            get
            {
                if (_request.HasFormContentType)
                    return _wrappers.Get(nameof(Form), () => new FormCollectionWrapper(_request.Form));
                else
                    return BslUndefinedValue.Instance;
            }
        }
    }
}
