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
        public string Accept
        {
            get => _items.Accept;
            set
            {
                _items.Accept = value;
            }
        }
        [ContextProperty("AcceptCharset", CanWrite = false)]
        public string AcceptCharset
        {
            get => _items.AcceptCharset;
            set
            {
                _items.AcceptCharset = value;
            }
        }
        [ContextProperty("AcceptEncoding", CanWrite = false)]
        public string AcceptEncoding
        {
            get => _items.AcceptEncoding;
            set
            {
                _items.AcceptEncoding = value;
            }
        }
        [ContextProperty("AcceptLanguage", CanWrite = false)]
        public string AcceptLanguage
        {
            get => _items.AcceptLanguage;
            set
            {
                _items.AcceptLanguage = value;
            }
        }
        [ContextProperty("AcceptRanges", CanWrite = false)]
        public string AcceptRanges
        {
            get => _items.AcceptRanges;
            set
            {
                _items.AcceptRanges = value;
            }
        }
        [ContextProperty("AccessControlAllowCredentials", CanWrite = false)]
        public string AccessControlAllowCredentials
        {
            get => _items.AccessControlAllowCredentials;
            set
            {
                _items.AccessControlAllowCredentials = value;
            }
        }
        [ContextProperty("AccessControlAllowHeaders", CanWrite = false)]
        public string AccessControlAllowHeaders
        {
            get => _items.AccessControlAllowHeaders;
            set
            {
                _items.AccessControlAllowHeaders = value;
            }
        }
        [ContextProperty("AccessControlAllowMethods", CanWrite = false)]
        public string AccessControlAllowMethods
        {
            get => _items.AccessControlAllowMethods;
            set
            {
                _items.AccessControlAllowMethods = value;
            }
        }
        [ContextProperty("AccessControlAllowOrigin", CanWrite = false)]
        public string AccessControlAllowOrigin
        {
            get => _items.AccessControlAllowOrigin;
            set
            {
                _items.AccessControlAllowOrigin = value;
            }
        }
        [ContextProperty("AccessControlExposeHeaders", CanWrite = false)]
        public string AccessControlExposeHeaders
        {
            get => _items.AccessControlExposeHeaders;
            set
            {
                _items.AccessControlExposeHeaders = value;
            }
        }
        [ContextProperty("AccessControlMaxAge", CanWrite = false)]
        public string AccessControlMaxAge
        {
            get => _items.AccessControlMaxAge;
            set
            {
                _items.AccessControlMaxAge = value;
            }
        }
        [ContextProperty("AccessControlRequestHeaders", CanWrite = false)]
        public string AccessControlRequestHeaders
        {
            get => _items.AccessControlRequestHeaders;
            set
            {
                _items.AccessControlRequestHeaders = value;
            }
        }
        [ContextProperty("AccessControlRequestMethod", CanWrite = false)]
        public string AccessControlRequestMethod
        {
            get => _items.AccessControlRequestMethod;
            set
            {
                _items.AccessControlRequestMethod = value;
            }
        }
        [ContextProperty("Age", CanWrite = false)]
        public string Age
        {
            get => _items.Age;
            set
            {
                _items.Age = value;
            }
        }
        [ContextProperty("Allow", CanWrite = false)]
        public string Allow
        {
            get => _items.Allow;
            set
            {
                _items.Allow = value;
            }
        }
        [ContextProperty("AltSvc", CanWrite = false)]
        public string AltSvc
        {
            get => _items.AltSvc;
            set
            {
                _items.AltSvc = value;
            }
        }
        [ContextProperty("Authorization", CanWrite = false)]
        public string Authorization
        {
            get => _items.Authorization;
            set
            {
                _items.Authorization = value;
            }
        }
        [ContextProperty("Baggage", CanWrite = false)]
        public string Baggage
        {
            get => _items.Baggage;
            set
            {
                _items.Baggage = value;
            }
        }
        [ContextProperty("CacheControl", CanWrite = false)]
        public string CacheControl
        {
            get => _items.CacheControl;
            set
            {
                _items.CacheControl = value;
            }
        }
        [ContextProperty("Connection", CanWrite = false)]
        public string Connection
        {
            get => _items.Connection;
            set
            {
                _items.Connection = value;
            }
        }
        [ContextProperty("ContentDisposition", CanWrite = false)]
        public string ContentDisposition
        {
            get => _items.ContentDisposition;
            set
            {
                _items.ContentDisposition = value;
            }
        }
        [ContextProperty("ContentEncoding", CanWrite = false)]
        public string ContentEncoding
        {
            get => _items.ContentEncoding;
            set
            {
                _items.ContentEncoding = value;
            }
        }
        [ContextProperty("ContentLanguage", CanWrite = false)]
        public string ContentLanguage
        {
            get => _items.ContentLanguage;
            set
            {
                _items.ContentLanguage = value;
            }
        }
        public long? ContentLength
        {
            get => _items.ContentLength;
            set
            {
                _items.ContentLength = value;
            }
        }
        [ContextProperty("ContentLocation", CanWrite = false)]
        public string ContentLocation
        {
            get => _items.ContentLocation;
            set
            {
                _items.ContentLocation = value;
            }
        }
        [ContextProperty("ContentMD5", CanWrite = false)]
        public string ContentMD5
        {
            get => _items.ContentMD5;
            set
            {
                _items.ContentMD5 = value;
            }
        }
        [ContextProperty("ContentRange", CanWrite = false)]
        public string ContentRange
        {
            get => _items.ContentRange;
            set
            {
                _items.ContentRange = value;
            }
        }
        [ContextProperty("ContentSecurityPolicy", CanWrite = false)]
        public string ContentSecurityPolicy
        {
            get => _items.ContentSecurityPolicy;
            set
            {
                _items.ContentSecurityPolicy = value;
            }
        }
        [ContextProperty("ContentSecurityPolicyReportOnly", CanWrite = false)]
        public string ContentSecurityPolicyReportOnly
        {
            get => _items.ContentSecurityPolicyReportOnly;
            set
            {
                _items.ContentSecurityPolicyReportOnly = value;
            }
        }
        [ContextProperty("ContentType", CanWrite = false)]
        public string ContentType
        {
            get => _items.ContentType;
            set
            {
                _items.ContentType = value;
            }
        }
        [ContextProperty("Cookie", CanWrite = false)]
        public string Cookie
        {
            get => _items.Cookie;
            set
            {
                _items.Cookie = value;
            }
        }
        [ContextProperty("CorrelationContext", CanWrite = false)]
        public string CorrelationContext
        {
            get => _items.CorrelationContext;
            set
            {
                _items.CorrelationContext = value;
            }
        }
        [ContextProperty("Date", CanWrite = false)]
        public string Date
        {
            get => _items.Date;
            set
            {
                _items.Date = value;
            }
        }
        [ContextProperty("ETag", CanWrite = false)]
        public string ETag
        {
            get => _items.ETag;
            set
            {
                _items.ETag = value;
            }
        }
        [ContextProperty("Expect", CanWrite = false)]
        public string Expect
        {
            get => _items.Expect;
            set
            {
                _items.Expect = value;
            }
        }
        [ContextProperty("Expires", CanWrite = false)]
        public string Expires
        {
            get => _items.Expires;
            set
            {
                _items.Expires = value;
            }
        }
        [ContextProperty("From", CanWrite = false)]
        public string From
        {
            get => _items.From;
            set
            {
                _items.From = value;
            }
        }
        [ContextProperty("GrpcAcceptEncoding", CanWrite = false)]
        public string GrpcAcceptEncoding
        {
            get => _items.GrpcAcceptEncoding;
            set
            {
                _items.GrpcAcceptEncoding = value;
            }
        }
        [ContextProperty("GrpcEncoding", CanWrite = false)]
        public string GrpcEncoding
        {
            get => _items.GrpcEncoding;
            set
            {
                _items.GrpcEncoding = value;
            }
        }
        [ContextProperty("GrpcMessage", CanWrite = false)]
        public string GrpcMessage
        {
            get => _items.GrpcMessage;
            set
            {
                _items.GrpcMessage = value;
            }
        }
        [ContextProperty("GrpcStatus", CanWrite = false)]
        public string GrpcStatus
        {
            get => _items.GrpcStatus;
            set
            {
                _items.GrpcStatus = value;
            }
        }
        [ContextProperty("GrpcTimeout", CanWrite = false)]
        public string GrpcTimeout
        {
            get => _items.GrpcTimeout;
            set
            {
                _items.GrpcTimeout = value;
            }
        }
        [ContextProperty("Host", CanWrite = false)]
        public string Host
        {
            get => _items.Host;
            set
            {
                _items.Host = value;
            }
        }
        [ContextProperty("IfMatch", CanWrite = false)]
        public string IfMatch
        {
            get => _items.IfMatch;
            set
            {
                _items.IfMatch = value;
            }
        }
        [ContextProperty("IfModifiedSince", CanWrite = false)]
        public string IfModifiedSince
        {
            get => _items.IfModifiedSince;
            set
            {
                _items.IfModifiedSince = value;
            }
        }
        [ContextProperty("IfNoneMatch", CanWrite = false)]
        public string IfNoneMatch
        {
            get => _items.IfNoneMatch;
            set
            {
                _items.IfNoneMatch = value;
            }
        }
        [ContextProperty("IfRange", CanWrite = false)]
        public string IfRange
        {
            get => _items.IfRange;
            set
            {
                _items.IfRange = value;
            }
        }
        [ContextProperty("IfUnmodifiedSince", CanWrite = false)]
        public string IfUnmodifiedSince
        {
            get => _items.IfUnmodifiedSince;
            set
            {
                _items.IfUnmodifiedSince = value;
            }
        }
        [ContextProperty("KeepAlive", CanWrite = false)]
        public string KeepAlive
        {
            get => _items.KeepAlive;
            set
            {
                _items.KeepAlive = value;
            }
        }
        [ContextProperty("LastModified", CanWrite = false)]
        public string LastModified
        {
            get => _items.LastModified;
            set
            {
                _items.LastModified = value;
            }
        }
        [ContextProperty("Link", CanWrite = false)]
        public string Link
        {
            get => _items.Link;
            set
            {
                _items.Link = value;
            }
        }
        [ContextProperty("Location", CanWrite = false)]
        public string Location
        {
            get => _items.Location;
            set
            {
                _items.Location = value;
            }
        }
        [ContextProperty("MaxForwards", CanWrite = false)]
        public string MaxForwards
        {
            get => _items.MaxForwards;
            set
            {
                _items.MaxForwards = value;
            }
        }
        [ContextProperty("Origin", CanWrite = false)]
        public string Origin
        {
            get => _items.Origin;
            set
            {
                _items.Origin = value;
            }
        }
        [ContextProperty("Pragma", CanWrite = false)]
        public string Pragma
        {
            get => _items.Pragma;
            set
            {
                _items.Pragma = value;
            }
        }
        [ContextProperty("ProxyAuthenticate", CanWrite = false)]
        public string ProxyAuthenticate
        {
            get => _items.ProxyAuthenticate;
            set
            {
                _items.ProxyAuthenticate = value;
            }
        }
        [ContextProperty("ProxyAuthorization", CanWrite = false)]
        public string ProxyAuthorization
        {
            get => _items.ProxyAuthorization;
            set
            {
                _items.ProxyAuthorization = value;
            }
        }
        [ContextProperty("ProxyConnection", CanWrite = false)]
        public string ProxyConnection
        {
            get => _items.ProxyConnection;
            set
            {
                _items.ProxyConnection = value;
            }
        }
        [ContextProperty("Range", CanWrite = false)]
        public string Range
        {
            get => _items.Range;
            set
            {
                _items.Range = value;
            }
        }
        [ContextProperty("Referer", CanWrite = false)]
        public string Referer
        {
            get => _items.Referer;
            set
            {
                _items.Referer = value;
            }
        }
        [ContextProperty("RequestId", CanWrite = false)]
        public string RequestId
        {
            get => _items.RequestId;
            set
            {
                _items.RequestId = value;
            }
        }
        [ContextProperty("RetryAfter", CanWrite = false)]
        public string RetryAfter
        {
            get => _items.RetryAfter;
            set
            {
                _items.RetryAfter = value;
            }
        }
        [ContextProperty("SecWebSocketAccept", CanWrite = false)]
        public string SecWebSocketAccept
        {
            get => _items.SecWebSocketAccept;
            set
            {
                _items.SecWebSocketAccept = value;
            }
        }
        [ContextProperty("SecWebSocketExtensions", CanWrite = false)]
        public string SecWebSocketExtensions
        {
            get => _items.SecWebSocketExtensions;
            set
            {
                _items.SecWebSocketExtensions = value;
            }
        }
        [ContextProperty("SecWebSocketKey", CanWrite = false)]
        public string SecWebSocketKey
        {
            get => _items.SecWebSocketKey;
            set
            {
                _items.SecWebSocketKey = value;
            }
        }
        [ContextProperty("SecWebSocketProtocol", CanWrite = false)]
        public string SecWebSocketProtocol
        {
            get => _items.SecWebSocketProtocol;
            set
            {
                _items.SecWebSocketProtocol = value;
            }
        }
        [ContextProperty("SecWebSocketVersion", CanWrite = false)]
        public string SecWebSocketVersion
        {
            get => _items.SecWebSocketVersion;
            set
            {
                _items.SecWebSocketVersion = value;
            }
        }
        [ContextProperty("Server", CanWrite = false)]
        public string Server
        {
            get => _items.Server;
            set
            {
                _items.Server = value;
            }
        }
        [ContextProperty("SetCookie", CanWrite = false)]
        public string SetCookie
        {
            get => _items.SetCookie;
            set
            {
                _items.SetCookie = value;
            }
        }
        [ContextProperty("StrictTransportSecurity", CanWrite = false)]
        public string StrictTransportSecurity
        {
            get => _items.StrictTransportSecurity;
            set
            {
                _items.StrictTransportSecurity = value;
            }
        }
        [ContextProperty("TE", CanWrite = false)]
        public string TE
        {
            get => _items.TE;
            set
            {
                _items.TE = value;
            }
        }
        [ContextProperty("TraceParent", CanWrite = false)]
        public string TraceParent
        {
            get => _items.TraceParent;
            set
            {
                _items.TraceParent = value;
            }
        }
        [ContextProperty("TraceState", CanWrite = false)]
        public string TraceState
        {
            get => _items.TraceState;
            set
            {
                _items.TraceState = value;
            }
        }
        [ContextProperty("Trailer", CanWrite = false)]
        public string Trailer
        {
            get => _items.Trailer;
            set
            {
                _items.Trailer = value;
            }
        }
        [ContextProperty("TransferEncoding", CanWrite = false)]
        public string TransferEncoding
        {
            get => _items.TransferEncoding;
            set
            {
                _items.TransferEncoding = value;
            }
        }
        [ContextProperty("Translate", CanWrite = false)]
        public string Translate
        {
            get => _items.Translate;
            set
            {
                _items.Translate = value;
            }
        }
        [ContextProperty("Upgrade", CanWrite = false)]
        public string Upgrade
        {
            get => _items.Upgrade;
            set
            {
                _items.Upgrade = value;
            }
        }
        [ContextProperty("UpgradeInsecureRequests", CanWrite = false)]
        public string UpgradeInsecureRequests
        {
            get => _items.UpgradeInsecureRequests;
            set
            {
                _items.UpgradeInsecureRequests = value;
            }
        }
        [ContextProperty("UserAgent", CanWrite = false)]
        public string UserAgent
        {
            get => _items.UserAgent;
            set
            {
                _items.UserAgent = value;
            }
        }
        [ContextProperty("Vary", CanWrite = false)]
        public string Vary
        {
            get => _items.Vary;
            set
            {
                _items.Vary = value;
            }
        }
        [ContextProperty("Via", CanWrite = false)]
        public string Via
        {
            get => _items.Via;
            set
            {
                _items.Via = value;
            }
        }
        [ContextProperty("Warning", CanWrite = false)]
        public string Warning
        {
            get => _items.Warning;
            set
            {
                _items.Warning = value;
            }
        }
        [ContextProperty("WebSocketSubProtocols", CanWrite = false)]
        public string WebSocketSubProtocols
        {
            get => _items.WebSocketSubProtocols;
            set
            {
                _items.WebSocketSubProtocols = value;
            }
        }
        [ContextProperty("WWWAuthenticate", CanWrite = false)]
        public string WWWAuthenticate
        {
            get => _items.WWWAuthenticate;
            set
            {
                _items.WWWAuthenticate = value;
            }
        }
        [ContextProperty("XContentTypeOptions", CanWrite = false)]
        public string XContentTypeOptions
        {
            get => _items.XContentTypeOptions;
            set
            {
                _items.XContentTypeOptions = value;
            }
        }
        [ContextProperty("XFrameOptions", CanWrite = false)]
        public string XFrameOptions
        {
            get => _items.XFrameOptions;
            set
            {
                _items.XFrameOptions = value;
            }
        }
        [ContextProperty("XPoweredBy", CanWrite = false)]
        public string XPoweredBy
        {
            get => _items.XPoweredBy;
            set
            {
                _items.XPoweredBy = value;
            }
        }
        [ContextProperty("XRequestedWith", CanWrite = false)]
        public string XRequestedWith
        {
            get => _items.XRequestedWith;
            set
            {
                _items.XRequestedWith = value;
            }
        }
        [ContextProperty("XUACompatible", CanWrite = false)]
        public string XUACompatible
        {
            get => _items.XUACompatible;
            set
            {
                _items.XUACompatible = value;
            }
        }
        [ContextProperty("XXSSProtection", CanWrite = false)]
        public string XXSSProtection
        {
            get => _items.XXSSProtection;
            set
            {
                _items.XXSSProtection = value;
            }
        }


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
            if (!_items.TryGetValue(index.AsString(), out var result))
                return ValueFactory.Create(result); 
            else
                return ValueFactory.Create();
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.SystemType != BasicTypes.Undefined)
                _items[index.AsString()] = val.AsString();
        }

        public override bool IsPropReadable(int propNum)
        {
            return false;
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
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
