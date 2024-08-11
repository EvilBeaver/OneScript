/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Web.Server;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using System.Linq;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;

namespace OneScript.Web.Server
{
    [ContextClass("СловарьЗаголовков", "HeaderDictionary")]
    public class HeaderDictionaryWrapper : AutoCollectionContext<HeaderDictionaryWrapper, KeyAndValueImpl>
    {
        private readonly IHeaderDictionary _items;


        [ContextProperty("Accept", CanWrite = false)]
        public string Accept => _items.Accept;

        [ContextProperty("AcceptCharset", CanWrite = false)]
        public string AcceptCharset => _items.AcceptCharset;

        [ContextProperty("AcceptEncoding", CanWrite = false)]
        public string AcceptEncoding => _items.AcceptEncoding;

        [ContextProperty("AcceptLanguage", CanWrite = false)]
        public string AcceptLanguage => _items.AcceptLanguage;

        [ContextProperty("AcceptRanges", CanWrite = false)]
        public string AcceptRanges => _items.AcceptRanges;

        [ContextProperty("AccessControlAllowCredentials", CanWrite = false)]
        public string AccessControlAllowCredentials => _items.AccessControlAllowCredentials;

        [ContextProperty("AccessControlAllowHeaders", CanWrite = false)]
        public string AccessControlAllowHeaders => _items.AccessControlAllowHeaders;

        [ContextProperty("AccessControlAllowMethods", CanWrite = false)]
        public string AccessControlAllowMethods => _items.AccessControlAllowMethods;

        [ContextProperty("AccessControlAllowOrigin", CanWrite = false)]
        public string AccessControlAllowOrigin => _items.AccessControlAllowOrigin;

        [ContextProperty("AccessControlExposeHeaders", CanWrite = false)]
        public string AccessControlExposeHeaders => _items.AccessControlExposeHeaders;

        [ContextProperty("AccessControlMaxAge", CanWrite = false)]
        public string AccessControlMaxAge => _items.AccessControlMaxAge;

        [ContextProperty("AccessControlRequestHeaders", CanWrite = false)]
        public string AccessControlRequestHeaders => _items.AccessControlRequestHeaders;

        [ContextProperty("AccessControlRequestMethod", CanWrite = false)]
        public string AccessControlRequestMethod => _items.AccessControlRequestMethod;

        [ContextProperty("Age", CanWrite = false)]
        public string Age => _items.Age;

        [ContextProperty("Allow", CanWrite = false)]
        public string Allow => _items.Allow;

        [ContextProperty("AltSvc", CanWrite = false)]
        public string AltSvc => _items.AltSvc;

        [ContextProperty("Authorization", CanWrite = false)]
        public string Authorization => _items.Authorization;

        [ContextProperty("Baggage", CanWrite = false)]
        public string Baggage => _items.Baggage;

        [ContextProperty("CacheControl", CanWrite = false)]
        public string CacheControl => _items.CacheControl;

        [ContextProperty("Connection", CanWrite = false)]
        public string Connection => _items.Connection;

        [ContextProperty("ContentDisposition", CanWrite = false)]
        public string ContentDisposition => _items.ContentDisposition;

        [ContextProperty("ContentEncoding", CanWrite = false)]
        public string ContentEncoding => _items.ContentEncoding;

        [ContextProperty("ContentLanguage", CanWrite = false)]
        public string ContentLanguage => _items.ContentLanguage;
        public long? ContentLength => _items.ContentLength;

        [ContextProperty("ContentLocation", CanWrite = false)]
        public string ContentLocation => _items.ContentLocation;

        [ContextProperty("ContentMD5", CanWrite = false)]
        public string ContentMD5 => _items.ContentMD5;

        [ContextProperty("ContentRange", CanWrite = false)]
        public string ContentRange => _items.ContentRange;

        [ContextProperty("ContentSecurityPolicy", CanWrite = false)]
        public string ContentSecurityPolicy => _items.ContentSecurityPolicy;

        [ContextProperty("ContentSecurityPolicyReportOnly", CanWrite = false)]
        public string ContentSecurityPolicyReportOnly => _items.ContentSecurityPolicyReportOnly;

        [ContextProperty("ContentType", CanWrite = false)]
        public string ContentType => _items.ContentType;

        [ContextProperty("Cookie", CanWrite = false)]
        public string Cookie => _items.Cookie;

        [ContextProperty("CorrelationContext", CanWrite = false)]
        public string CorrelationContext => _items.CorrelationContext;

        [ContextProperty("Date", CanWrite = false)]
        public string Date => _items.Date;

        [ContextProperty("ETag", CanWrite = false)]
        public string ETag => _items.ETag;

        [ContextProperty("Expect", CanWrite = false)]
        public string Expect => _items.Expect;

        [ContextProperty("Expires", CanWrite = false)]
        public string Expires => _items.Expires;

        [ContextProperty("From", CanWrite = false)]
        public string From => _items.From;

        [ContextProperty("GrpcAcceptEncoding", CanWrite = false)]
        public string GrpcAcceptEncoding => _items.GrpcAcceptEncoding;

        [ContextProperty("GrpcEncoding", CanWrite = false)]
        public string GrpcEncoding => _items.GrpcEncoding;

        [ContextProperty("GrpcMessage", CanWrite = false)]
        public string GrpcMessage => _items.GrpcMessage;

        [ContextProperty("GrpcStatus", CanWrite = false)]
        public string GrpcStatus => _items.GrpcStatus;

        [ContextProperty("GrpcTimeout", CanWrite = false)]
        public string GrpcTimeout => _items.GrpcTimeout;

        [ContextProperty("Host", CanWrite = false)]
        public string Host => _items.Host;

        [ContextProperty("IfMatch", CanWrite = false)]
        public string IfMatch => _items.IfMatch;

        [ContextProperty("IfModifiedSince", CanWrite = false)]
        public string IfModifiedSince => _items.IfModifiedSince;

        [ContextProperty("IfNoneMatch", CanWrite = false)]
        public string IfNoneMatch => _items.IfNoneMatch;

        [ContextProperty("IfRange", CanWrite = false)]
        public string IfRange => _items.IfRange;

        [ContextProperty("IfUnmodifiedSince", CanWrite = false)]
        public string IfUnmodifiedSince => _items.IfUnmodifiedSince;

        [ContextProperty("KeepAlive", CanWrite = false)]
        public string KeepAlive => _items.KeepAlive;

        [ContextProperty("LastModified", CanWrite = false)]
        public string LastModified => _items.LastModified;

        [ContextProperty("Link", CanWrite = false)]
        public string Link => _items.Link;

        [ContextProperty("Location", CanWrite = false)]
        public string Location => _items.Location;

        [ContextProperty("MaxForwards", CanWrite = false)]
        public string MaxForwards => _items.MaxForwards;

        [ContextProperty("Origin", CanWrite = false)]
        public string Origin => _items.Origin;

        [ContextProperty("Pragma", CanWrite = false)]
        public string Pragma => _items.Pragma;

        [ContextProperty("ProxyAuthenticate", CanWrite = false)]
        public string ProxyAuthenticate => _items.ProxyAuthenticate;

        [ContextProperty("ProxyAuthorization", CanWrite = false)]
        public string ProxyAuthorization => _items.ProxyAuthorization;

        [ContextProperty("ProxyConnection", CanWrite = false)]
        public string ProxyConnection => _items.ProxyConnection;

        [ContextProperty("Range", CanWrite = false)]
        public string Range => _items.Range;

        [ContextProperty("Referer", CanWrite = false)]
        public string Referer => _items.Referer;

        [ContextProperty("RequestId", CanWrite = false)]
        public string RequestId => _items.RequestId;

        [ContextProperty("RetryAfter", CanWrite = false)]
        public string RetryAfter => _items.RetryAfter;

        [ContextProperty("SecWebSocketAccept", CanWrite = false)]
        public string SecWebSocketAccept => _items.SecWebSocketAccept;

        [ContextProperty("SecWebSocketExtensions", CanWrite = false)]
        public string SecWebSocketExtensions => _items.SecWebSocketExtensions;

        [ContextProperty("SecWebSocketKey", CanWrite = false)]
        public string SecWebSocketKey => _items.SecWebSocketKey;

        [ContextProperty("SecWebSocketProtocol", CanWrite = false)]
        public string SecWebSocketProtocol => _items.SecWebSocketProtocol;

        [ContextProperty("SecWebSocketVersion", CanWrite = false)]
        public string SecWebSocketVersion => _items.SecWebSocketVersion;

        [ContextProperty("Server", CanWrite = false)]
        public string Server => _items.Server;

        [ContextProperty("SetCookie", CanWrite = false)]
        public string SetCookie => _items.SetCookie;

        [ContextProperty("StrictTransportSecurity", CanWrite = false)]
        public string StrictTransportSecurity => _items.StrictTransportSecurity;

        [ContextProperty("TE", CanWrite = false)]
        public string TE => _items.TE;

        [ContextProperty("TraceParent", CanWrite = false)]
        public string TraceParent => _items.TraceParent;

        [ContextProperty("TraceState", CanWrite = false)]
        public string TraceState => _items.TraceState;

        [ContextProperty("Trailer", CanWrite = false)]
        public string Trailer => _items.Trailer;

        [ContextProperty("TransferEncoding", CanWrite = false)]
        public string TransferEncoding => _items.TransferEncoding;

        [ContextProperty("Translate", CanWrite = false)]
        public string Translate => _items.Translate;

        [ContextProperty("Upgrade", CanWrite = false)]
        public string Upgrade => _items.Upgrade;

        [ContextProperty("UpgradeInsecureRequests", CanWrite = false)]
        public string UpgradeInsecureRequests => _items.UpgradeInsecureRequests;

        [ContextProperty("UserAgent", CanWrite = false)]
        public string UserAgent => _items.UserAgent;

        [ContextProperty("Vary", CanWrite = false)]
        public string Vary => _items.Vary;

        [ContextProperty("Via", CanWrite = false)]
        public string Via => _items.Via;

        [ContextProperty("Warning", CanWrite = false)]
        public string Warning => _items.Warning;

        [ContextProperty("WebSocketSubProtocols", CanWrite = false)]
        public string WebSocketSubProtocols => _items.WebSocketSubProtocols;

        [ContextProperty("WWWAuthenticate", CanWrite = false)]
        public string WWWAuthenticate => _items.WWWAuthenticate;

        [ContextProperty("XContentTypeOptions", CanWrite = false)]
        public string XContentTypeOptions => _items.XContentTypeOptions;

        [ContextProperty("XFrameOptions", CanWrite = false)]
        public string XFrameOptions => _items.XFrameOptions;

        [ContextProperty("XPoweredBy", CanWrite = false)]
        public string XPoweredBy => _items.XPoweredBy;

        [ContextProperty("XRequestedWith", CanWrite = false)]
        public string XRequestedWith => _items.XRequestedWith;

        [ContextProperty("XUACompatible", CanWrite = false)]
        public string XUACompatible => _items.XUACompatible;

        [ContextProperty("XXSSProtection", CanWrite = false)]
        public string XXSSProtection => _items.XXSSProtection;

        public HeaderDictionaryWrapper(IHeaderDictionary headers)
        {
            _items = headers;
        }

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (_items.TryGetValue(index.AsString(), out var result))
                return ValueFactory.Create(result); 
            else
                return ValueFactory.Create();
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.SystemType != BasicTypes.Undefined)
                _items[index.AsString()] = val.AsString();
        }

        internal bool ContainsKey(IValue key)
        {
            return _items.ContainsKey(key.AsString());
        }

        public IEnumerable<IValue> Keys()
        {
            foreach (var key in _items.Keys)
                yield return ValueFactory.Create(key);
        }

        #region ICollectionContext Members

        [ContextMethod("Получить", "Get")]
        public IValue Retrieve(IValue key)
        {
            return GetIndexedValue(key);
        }

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _items.Count;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _items.Clear();
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue key)
        {
            _items.Remove(key.AsString());
        }
        #endregion

        #region IEnumerable<IValue> Members

        public override IEnumerator<KeyAndValueImpl> GetEnumerator()
        {
            foreach (var item in _items)
            {
                yield return new KeyAndValueImpl(ValueFactory.Create(item.Key), ValueFactory.Create(item.Value));
            }
        }

        #endregion

        [ContextMethod("Добавить", "Append")]
        public void Append(string Key, string Value)
            => _items.Append(Key, Value);

        [ContextMethod("ДобавитьСписок", "AppendList")]
        public void AppendList(string Key, ArrayImpl Values)
            => _items.AppendList(Key, Values.Select(i => i.AsString()).ToList());

        [ContextMethod("ДобавитьРазделенныеЗапятымиЗначения", "AppendCommaSeparatedValues")]
        public void AppendCommaSeparated(string Key, ArrayImpl Values)
            => _items.AppendCommaSeparatedValues(Key, Values.Select(i => i.AsString()).ToArray());
    }
}
