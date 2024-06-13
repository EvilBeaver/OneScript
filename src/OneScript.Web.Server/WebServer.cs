/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;

namespace OneScript.Web.Server
{
    [ContextClass("ВебСервер", "WebServer")]
    public class WebServer: AutoContext<WebServer>
    {
        private readonly ExecutionContext _executionContext;
        private WebApplication _app;
        private readonly List<(IRuntimeContextInstance Target, string MethodName)> _middlewares = new List<(IRuntimeContextInstance Target, string MethodName)>();
        private string _contentRoot = null;
        private bool _useStaticFiles = false;
        private (IRuntimeContextInstance Target, string MethodName)? _exceptionHandler = null;

        [ContextProperty("Порт", "Port", CanWrite = false)]
        public int Port { get; private set; }

        public WebServer(ExecutionContext executionContext) 
        {
            _executionContext = executionContext;
        }

        [ScriptConstructor(Name = "С портом по умолчанию - 8080")]
        public static WebServer Constructor(TypeActivationContext typeActivationContext)
        {
            var server = new WebServer(typeActivationContext.Services.Resolve<ExecutionContext>())
            {
                Port = 8080
            };

            return server;
        }

        [ScriptConstructor(Name = "С указанием порта прослушивателя")]
        public static WebServer Constructor(TypeActivationContext typeActivationContext, IValue port)
        {
            var server = new WebServer(typeActivationContext.Services.Resolve<ExecutionContext>())
            {
                Port = (int)port.AsNumber()
            };

            return server;
        }

        [ContextMethod("Запустить", "Run")]
        public void Run()
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.AllowSynchronousIO = true;
                options.ListenAnyIP(Port);
            });

            if (_contentRoot != null)
                builder.WebHost.UseContentRoot(_contentRoot);

            _app = builder.Build();

            if (_useStaticFiles)
                _app.UseStaticFiles();

            if (_exceptionHandler != null)
                _app.UseExceptionHandler(handler =>
                {
                    handler.Run(context =>
                    {
                        var args = new IValue[]
                        {
                            new HttpContextWrapper(context),
                        };

                        var methodNumber = _exceptionHandler?.Target.GetMethodNumber(_exceptionHandler?.MethodName);

                        var debugController = _executionContext.Services.TryResolve<IDebugController>();
                        debugController?.AttachToThread();

                        try
                        {
                            _exceptionHandler?.Target.CallAsProcedure((int)methodNumber, args);
                        }
                        catch (Exception ex)
                        {
                            WriteExceptionToResponse(context, ex);
                        }
                        finally
                        {
                            debugController?.DetachFromThread();
                        }

                        return Task.CompletedTask;
                    });
                });

            _middlewares.ForEach(middleware =>
            {
                _app.Use((context, next) =>
                {
                    var args = new IValue[]
                    {
                        new HttpContextWrapper(context),
                        new RequestDelegateWrapper(next)
                    };

                    var methodNumber = middleware.Target.GetMethodNumber(middleware.MethodName);

                    var debugController = _executionContext.Services.TryResolve<IDebugController>();
                    debugController?.AttachToThread();

                    try
                    {
                        middleware.Target.CallAsProcedure(methodNumber, args);          
                    }
                    catch (Exception ex)
                    {
                        if (_exceptionHandler == null)
                            WriteExceptionToResponse(context, ex);
                        else
                            throw;
                    }
                    finally
                    {
                        debugController?.DetachFromThread();
                    }

                    return Task.CompletedTask;
                });
            });

            _app.Run();
        }

        private static void WriteExceptionToResponse(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.StatusCode = 500;
            httpContext.Response.ContentType = "text/plain;charset=utf-8";
            httpContext.Response.WriteAsync(ex.Message).Wait();
        }

        [ContextMethod("ДобавитьОбработчикЗапросов", "AddRequestsHandler")]
        public void SetRequestsHandler(IRuntimeContextInstance target, string methodName)
            => _middlewares.Add((target, methodName));

        [ContextMethod("ДобавитьОбработчикИсключений", "AddExceptionsHandler")]
        public void SetExceptionsHandler(IRuntimeContextInstance target, string methodName)
            => _exceptionHandler = (target, methodName);

        [ContextMethod("УстановитьКорневойПутьСодержимого", "SetContentRoot")]
        public void SetContentRoot(IValue path)
        {
            _contentRoot = path.AsString();
        }

        [ContextMethod("ИспользоватьСтатическиеФайлы", "UseStaticFiles")]
        public void UseStaticFiles()
        {
            _useStaticFiles = true;
        }
    }
}
