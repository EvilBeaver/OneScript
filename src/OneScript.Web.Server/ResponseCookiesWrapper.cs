﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.Web.Server
{
    [ContextClass("КукиОтвета", "ResponseCookies")]
    public class ResponseCookiesWrapper : AutoContext<ResponseCookiesWrapper>
    {
        private readonly IResponseCookies _items;

        public ResponseCookiesWrapper(IResponseCookies headers)
        {
            _items = headers;
        }

        [ContextMethod("Добавить", "Append")]
        public void Append(IValue key, IValue value, CookieOptionsWrapper cookieOptions = null)
        {
            if (cookieOptions is null)
                _items.Append(key.AsString(), value.AsString());
            else
                _items.Append(key.AsString(), value.AsString(), cookieOptions._cookieOptions);
        }

        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue key, CookieOptionsWrapper cookieOptions = null)
        {
            if (cookieOptions is null)
                _items.Delete(key.AsString());
            else
                _items.Delete(key.AsString(), cookieOptions._cookieOptions);
        }
    }
}
