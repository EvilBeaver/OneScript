/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace OneScript.Web.Server
{
    [ContextClass("ДелегатЗапроса", "RequestDelegate")]
    public class RequestDelegateWrapper : AutoContext<RequestDelegateWrapper>
    {
        private readonly RequestDelegate _delegate;

        public RequestDelegateWrapper(RequestDelegate item)
        {
            _delegate = item;
        }

        [ContextMethod("Вызвать", "Invoke")]
        public void Invoke(HttpContextWrapper httpContext)
        {
            _delegate.Invoke(httpContext.GetContext());
        }
    }
}
