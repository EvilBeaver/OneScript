/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;

namespace OneScript.Web.Server
{
    [ContextClass("ПараметрыCookie", "CookieOptions")]
    public class CookieOptionsWrapper: AutoContext<CookieOptionsWrapper>
    {
        internal readonly CookieOptions _cookieOptions = new CookieOptions();

        [ContextProperty("Домен", "Domain")]
        public IValue Domain
        {
            get
            {
                if (_cookieOptions.Domain == null)
                    return BslNullValue.Instance;
                else
                    return BslStringValue.Create(_cookieOptions.Domain);
            }
            set
            {
                if (value is BslNullValue)
                    _cookieOptions.Domain = null;
                else
                    _cookieOptions.Domain = value.AsString();
            }
        }

        [ContextProperty("Путь", "Path")]
        public IValue Path
        {
            get
            {
                if (_cookieOptions.Path == null)
                    return BslNullValue.Instance;
                else
                    return BslStringValue.Create(_cookieOptions.Path);
            }
            set
            {
                if (value is BslNullValue)
                    _cookieOptions.Path = null;
                else
                    _cookieOptions.Path = value.AsString();
            }
        }

        [ContextProperty("Истекает", "Expires")]
        public IValue Expires
        {
            get
            {
                if (_cookieOptions.Expires.HasValue)
                    return BslDateValue.Create(_cookieOptions.Expires.Value.UtcDateTime);
                else
                    return BslNullValue.Instance;
            }
            set
            {
                if (value is BslNullValue)
                    _cookieOptions.Expires = null;
                else
                    _cookieOptions.Expires = new DateTimeOffset(value.AsDate());
            }
        }

        [ContextProperty("Безопасный", "Secure")]
        public IValue Secure
        {
            get => BslBooleanValue.Create(_cookieOptions.Secure);
            set => _cookieOptions.Secure = value.AsBoolean();
        }

        [ContextProperty("РежимSameSite", "SameSiteMode")]
        public SameSiteModeEnum SameSiteMode
        {
            get => (SameSiteModeEnum)_cookieOptions.SameSite;
            set => _cookieOptions.SameSite = (Microsoft.AspNetCore.Http.SameSiteMode)value;
        }

        [ContextProperty("ТолькоHttp", "HttpOnly")]
        public IValue HttpOnly
        {
            get => BslBooleanValue.Create(_cookieOptions.HttpOnly);
            set => _cookieOptions.HttpOnly = value.AsBoolean();
        }

        [ContextProperty("МаксимальныйВозраст", "MaxAge")]
        public IValue MaxAge
        {
            get
            {
                if (_cookieOptions.MaxAge.HasValue)
                    return BslNumericValue.Create((decimal)_cookieOptions.MaxAge.Value.TotalSeconds);
                else
                    return BslNullValue.Instance;
            }
            set
            {
                if (value is BslNullValue)
                    _cookieOptions.MaxAge = null;
                else
                    _cookieOptions.MaxAge = TimeSpan.FromSeconds((double)value.AsNumber());
            }
        }

        [ContextProperty("Важный", "IsEssential")]
        public IValue IsEssential
        {
            get => BslBooleanValue.Create(_cookieOptions.IsEssential);
            set => _cookieOptions.IsEssential = value.AsBoolean();
        }
    }
}
