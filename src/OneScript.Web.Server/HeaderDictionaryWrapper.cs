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
using Microsoft.Extensions.Primitives;

namespace OneScript.Web.Server
{
    [ContextClass("СловарьЗаголовков", "HeaderDictionary")]
    public class HeaderDictionaryWrapper : AutoCollectionContext<HeaderDictionaryWrapper, KeyAndValueImpl>
    {
        private readonly IHeaderDictionary _items;

        [ContextProperty("Accept", CanWrite = false)]
        public StringValuesWrapper Accept => _items.Accept;

        [ContextProperty("AcceptCharset", CanWrite = false)]
        public StringValuesWrapper AcceptCharset => _items.AcceptCharset;

        [ContextProperty("AcceptEncoding", CanWrite = false)]
        public StringValuesWrapper AcceptEncoding => _items.AcceptEncoding;

        [ContextProperty("AcceptLanguage", CanWrite = false)]
        public StringValuesWrapper AcceptLanguage => _items.AcceptLanguage;

        [ContextProperty("AcceptRanges", CanWrite = false)]
        public StringValuesWrapper AcceptRanges => _items.AcceptRanges;

        [ContextProperty("AccessControlAllowCredentials", CanWrite = false)]
        public StringValuesWrapper AccessControlAllowCredentials => _items.AccessControlAllowCredentials;

        [ContextProperty("AccessControlAllowHeaders", CanWrite = false)]
        public StringValuesWrapper AccessControlAllowHeaders => _items.AccessControlAllowHeaders;

        [ContextProperty("AccessControlAllowMethods", CanWrite = false)]
        public StringValuesWrapper AccessControlAllowMethods => _items.AccessControlAllowMethods;

        [ContextProperty("AccessControlAllowOrigin", CanWrite = false)]
        public StringValuesWrapper AccessControlAllowOrigin => _items.AccessControlAllowOrigin;

        [ContextProperty("AccessControlExposeHeaders", CanWrite = false)]
        public StringValuesWrapper AccessControlExposeHeaders => _items.AccessControlExposeHeaders;

        [ContextProperty("AccessControlMaxAge", CanWrite = false)]
        public StringValuesWrapper AccessControlMaxAge => _items.AccessControlMaxAge;

        [ContextProperty("AccessControlRequestHeaders", CanWrite = false)]
        public StringValuesWrapper AccessControlRequestHeaders => _items.AccessControlRequestHeaders;

        [ContextProperty("AccessControlRequestMethod", CanWrite = false)]
        public StringValuesWrapper AccessControlRequestMethod => _items.AccessControlRequestMethod;

        [ContextProperty("Age", CanWrite = false)]
        public StringValuesWrapper Age => _items.Age;

        [ContextProperty("Allow", CanWrite = false)]
        public StringValuesWrapper Allow => _items.Allow;

        [ContextProperty("AltSvc", CanWrite = false)]
        public StringValuesWrapper AltSvc => _items.AltSvc;

        [ContextProperty("Authorization", CanWrite = false)]
        public StringValuesWrapper Authorization => _items.Authorization;

        [ContextProperty("Baggage", CanWrite = false)]
        public StringValuesWrapper Baggage => _items.Baggage;

        [ContextProperty("CacheControl", CanWrite = false)]
        public StringValuesWrapper CacheControl => _items.CacheControl;

        [ContextProperty("Connection", CanWrite = false)]
        public StringValuesWrapper Connection => _items.Connection;

        [ContextProperty("ContentDisposition", CanWrite = false)]
        public StringValuesWrapper ContentDisposition => _items.ContentDisposition;

        [ContextProperty("ContentEncoding", CanWrite = false)]
        public StringValuesWrapper ContentEncoding => _items.ContentEncoding;

        [ContextProperty("ContentLanguage", CanWrite = false)]
        public StringValuesWrapper ContentLanguage => _items.ContentLanguage;
        public long? ContentLength => _items.ContentLength;

        [ContextProperty("ContentLocation", CanWrite = false)]
        public StringValuesWrapper ContentLocation => _items.ContentLocation;

        [ContextProperty("ContentMD5", CanWrite = false)]
        public StringValuesWrapper ContentMD5 => _items.ContentMD5;

        [ContextProperty("ContentRange", CanWrite = false)]
        public StringValuesWrapper ContentRange => _items.ContentRange;

        [ContextProperty("ContentSecurityPolicy", CanWrite = false)]
        public StringValuesWrapper ContentSecurityPolicy => _items.ContentSecurityPolicy;

        [ContextProperty("ContentSecurityPolicyReportOnly", CanWrite = false)]
        public StringValuesWrapper ContentSecurityPolicyReportOnly => _items.ContentSecurityPolicyReportOnly;

        [ContextProperty("ContentType", CanWrite = false)]
        public StringValuesWrapper ContentType => _items.ContentType;

        [ContextProperty("Cookie", CanWrite = false)]
        public StringValuesWrapper Cookie => _items.Cookie;

        [ContextProperty("CorrelationContext", CanWrite = false)]
        public StringValuesWrapper CorrelationContext => _items.CorrelationContext;

        [ContextProperty("Date", CanWrite = false)]
        public StringValuesWrapper Date => _items.Date;

        [ContextProperty("ETag", CanWrite = false)]
        public StringValuesWrapper ETag => _items.ETag;

        [ContextProperty("Expect", CanWrite = false)]
        public StringValuesWrapper Expect => _items.Expect;

        [ContextProperty("Expires", CanWrite = false)]
        public StringValuesWrapper Expires => _items.Expires;

        [ContextProperty("From", CanWrite = false)]
        public StringValuesWrapper From => _items.From;

        [ContextProperty("GrpcAcceptEncoding", CanWrite = false)]
        public StringValuesWrapper GrpcAcceptEncoding => _items.GrpcAcceptEncoding;

        [ContextProperty("GrpcEncoding", CanWrite = false)]
        public StringValuesWrapper GrpcEncoding => _items.GrpcEncoding;

        [ContextProperty("GrpcMessage", CanWrite = false)]
        public StringValuesWrapper GrpcMessage => _items.GrpcMessage;

        [ContextProperty("GrpcStatus", CanWrite = false)]
        public StringValuesWrapper GrpcStatus => _items.GrpcStatus;

        [ContextProperty("GrpcTimeout", CanWrite = false)]
        public StringValuesWrapper GrpcTimeout => _items.GrpcTimeout;

        [ContextProperty("Host", CanWrite = false)]
        public StringValuesWrapper Host => _items.Host;

        [ContextProperty("IfMatch", CanWrite = false)]
        public StringValuesWrapper IfMatch => _items.IfMatch;

        [ContextProperty("IfModifiedSince", CanWrite = false)]
        public StringValuesWrapper IfModifiedSince => _items.IfModifiedSince;

        [ContextProperty("IfNoneMatch", CanWrite = false)]
        public StringValuesWrapper IfNoneMatch => _items.IfNoneMatch;

        [ContextProperty("IfRange", CanWrite = false)]
        public StringValuesWrapper IfRange => _items.IfRange;

        [ContextProperty("IfUnmodifiedSince", CanWrite = false)]
        public StringValuesWrapper IfUnmodifiedSince => _items.IfUnmodifiedSince;

        [ContextProperty("KeepAlive", CanWrite = false)]
        public StringValuesWrapper KeepAlive => _items.KeepAlive;

        [ContextProperty("LastModified", CanWrite = false)]
        public StringValuesWrapper LastModified => _items.LastModified;

        [ContextProperty("Link", CanWrite = false)]
        public StringValuesWrapper Link => _items.Link;

        [ContextProperty("Location", CanWrite = false)]
        public StringValuesWrapper Location => _items.Location;

        [ContextProperty("MaxForwards", CanWrite = false)]
        public StringValuesWrapper MaxForwards => _items.MaxForwards;

        [ContextProperty("Origin", CanWrite = false)]
        public StringValuesWrapper Origin => _items.Origin;

        [ContextProperty("Pragma", CanWrite = false)]
        public StringValuesWrapper Pragma => _items.Pragma;

        [ContextProperty("ProxyAuthenticate", CanWrite = false)]
        public StringValuesWrapper ProxyAuthenticate => _items.ProxyAuthenticate;

        [ContextProperty("ProxyAuthorization", CanWrite = false)]
        public StringValuesWrapper ProxyAuthorization => _items.ProxyAuthorization;

        [ContextProperty("ProxyConnection", CanWrite = false)]
        public StringValuesWrapper ProxyConnection => _items.ProxyConnection;

        [ContextProperty("Range", CanWrite = false)]
        public StringValuesWrapper Range => _items.Range;

        [ContextProperty("Referer", CanWrite = false)]
        public StringValuesWrapper Referer => _items.Referer;

        [ContextProperty("RequestId", CanWrite = false)]
        public StringValuesWrapper RequestId => _items.RequestId;

        [ContextProperty("RetryAfter", CanWrite = false)]
        public StringValuesWrapper RetryAfter => _items.RetryAfter;

        [ContextProperty("SecWebSocketAccept", CanWrite = false)]
        public StringValuesWrapper SecWebSocketAccept => _items.SecWebSocketAccept;

        [ContextProperty("SecWebSocketExtensions", CanWrite = false)]
        public StringValuesWrapper SecWebSocketExtensions => _items.SecWebSocketExtensions;

        [ContextProperty("SecWebSocketKey", CanWrite = false)]
        public StringValuesWrapper SecWebSocketKey => _items.SecWebSocketKey;

        [ContextProperty("SecWebSocketProtocol", CanWrite = false)]
        public StringValuesWrapper SecWebSocketProtocol => _items.SecWebSocketProtocol;

        [ContextProperty("SecWebSocketVersion", CanWrite = false)]
        public StringValuesWrapper SecWebSocketVersion => _items.SecWebSocketVersion;

        [ContextProperty("Server", CanWrite = false)]
        public StringValuesWrapper Server => _items.Server;

        [ContextProperty("SetCookie", CanWrite = false)]
        public StringValuesWrapper SetCookie => _items.SetCookie;

        [ContextProperty("StrictTransportSecurity", CanWrite = false)]
        public StringValuesWrapper StrictTransportSecurity => _items.StrictTransportSecurity;

        [ContextProperty("TE", CanWrite = false)]
        public StringValuesWrapper TE => _items.TE;

        [ContextProperty("TraceParent", CanWrite = false)]
        public StringValuesWrapper TraceParent => _items.TraceParent;

        [ContextProperty("TraceState", CanWrite = false)]
        public StringValuesWrapper TraceState => _items.TraceState;

        [ContextProperty("Trailer", CanWrite = false)]
        public StringValuesWrapper Trailer => _items.Trailer;

        [ContextProperty("TransferEncoding", CanWrite = false)]
        public StringValuesWrapper TransferEncoding => _items.TransferEncoding;

        [ContextProperty("Translate", CanWrite = false)]
        public StringValuesWrapper Translate => _items.Translate;

        [ContextProperty("Upgrade", CanWrite = false)]
        public StringValuesWrapper Upgrade => _items.Upgrade;

        [ContextProperty("UpgradeInsecureRequests", CanWrite = false)]
        public StringValuesWrapper UpgradeInsecureRequests => _items.UpgradeInsecureRequests;

        [ContextProperty("UserAgent", CanWrite = false)]
        public StringValuesWrapper UserAgent => _items.UserAgent;

        [ContextProperty("Vary", CanWrite = false)]
        public StringValuesWrapper Vary => _items.Vary;

        [ContextProperty("Via", CanWrite = false)]
        public StringValuesWrapper Via => _items.Via;

        [ContextProperty("Warning", CanWrite = false)]
        public StringValuesWrapper Warning => _items.Warning;

        [ContextProperty("WebSocketSubProtocols", CanWrite = false)]
        public StringValuesWrapper WebSocketSubProtocols => _items.WebSocketSubProtocols;

        [ContextProperty("WWWAuthenticate", CanWrite = false)]
        public StringValuesWrapper WWWAuthenticate => _items.WWWAuthenticate;

        [ContextProperty("XContentTypeOptions", CanWrite = false)]
        public StringValuesWrapper XContentTypeOptions => _items.XContentTypeOptions;

        [ContextProperty("XFrameOptions", CanWrite = false)]
        public StringValuesWrapper XFrameOptions => _items.XFrameOptions;

        [ContextProperty("XPoweredBy", CanWrite = false)]
        public StringValuesWrapper XPoweredBy => _items.XPoweredBy;

        [ContextProperty("XRequestedWith", CanWrite = false)]
        public StringValuesWrapper XRequestedWith => _items.XRequestedWith;

        [ContextProperty("XUACompatible", CanWrite = false)]
        public StringValuesWrapper XUACompatible => _items.XUACompatible;

        [ContextProperty("XXSSProtection", CanWrite = false)]
        public StringValuesWrapper XXSSProtection => _items.XXSSProtection;

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

        public override StringValuesWrapper GetIndexedValue(IValue index)
        {
            if (_items.TryGetValue(index.AsString(), out var result))
                return result;
            else
                return StringValues.Empty;
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
                yield return new KeyAndValueImpl(ValueFactory.Create(item.Key), (StringValuesWrapper)item.Value);
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
