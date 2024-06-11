using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Http.Web
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
            _delegate.Invoke(httpContext._context);
        }
    }
}
