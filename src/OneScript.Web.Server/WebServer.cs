/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;
using Microsoft.Extensions.Configuration;

namespace OneScript.Web.Server
{
    [ContextClass("ВебСервер", "WebServer")]
    public class WebServer: AutoContext<WebServer>
    {
        private readonly ExecutionContext _executionContext;
        private WebApplication _app;
        private readonly List<(IRuntimeContextInstance Target, string MethodName)> _middlewares = new List<(IRuntimeContextInstance Target, string MethodName)>();
        
        private string _contentRoot = null;
        private string _wwwRoot;
        
        private bool _useStaticFiles = false;
        private bool _useWebSockets = false;
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
            ConfigureApp();

            _app.Run();
        }

        /// <summary>
        /// Останавливает веб-сервер
        /// </summary>
        [ContextMethod("Остановить", "Stop")]
        public void Stop()
        {
            _app?.StopAsync().Wait();
        }

        private void ConfigureApp()
        {
            var appOptions = new WebApplicationOptions
            {
                ContentRootPath = _contentRoot,
                WebRootPath = _wwwRoot
            };
            
            var builder = WebApplication.CreateBuilder(appOptions);
            builder.WebHost.ConfigureKestrel(options =>
            {
                var kestrelSection = builder.Configuration.GetSection("Kestrel");
                options.Configure(kestrelSection);
                kestrelSection.Bind(options);

                options.AllowSynchronousIO = true;
                options.ListenAnyIP(Port);
            });

            _app = builder.Build();

            if (_useStaticFiles)
                _app.UseStaticFiles();

            if (_exceptionHandler != null)
            {
                UseBslExceptionHandler();
            }
            else
            {
                UseDefaultExceptionHandler();
            }
            
            if (_useWebSockets)
                _app.UseWebSockets();

            _middlewares.ForEach(middleware =>
            {
                _app.Use((context, next) =>
                {
                    var args = new IValue[]
                    {
                        new HttpContextWrapper(_executionContext.TypeManager, context),
                        new RequestDelegateWrapper(next)
                    };

                    var methodNumber = middleware.Target.GetMethodNumber(middleware.MethodName);

                    var debugController = _executionContext.Services.TryResolve<IDebugController>();
                    debugController?.AttachToThread();

                    try
                    {
                        middleware.Target.CallAsProcedure(methodNumber, args);
                    }
                    finally
                    {
                        debugController?.DetachFromThread();
                    }

                    return Task.CompletedTask;
                });
            });
        }

        private void UseDefaultExceptionHandler()
        {
            _app.UseExceptionHandler(errApp =>
            {
                errApp.Run(context =>
                {
                    var exceptionHandlerPathFeature =
                        context.Features.Get<IExceptionHandlerPathFeature>();

                    WriteExceptionToResponse(context, exceptionHandlerPathFeature?.Error);

                    return Task.CompletedTask;
                });
            });
        }

        private void UseBslExceptionHandler()
        {
            _app.UseExceptionHandler(handler =>
            {
                handler.Run(context =>
                {
                    var args = new IValue[]
                    {
                        new HttpContextWrapper(_executionContext.TypeManager, context),
                    };

                    var methodNumber = _exceptionHandler?.Target.GetMethodNumber(_exceptionHandler?.MethodName);

                    var debugController = _executionContext.Services.TryResolve<IDebugController>();

                    // Thread unsafe call!
                    debugController?.AttachToThread();

                    try
                    {
                        _exceptionHandler?.Target.CallAsProcedure((int)methodNumber, args);
                    }
                    catch (Exception ex)
                    {
                        if (!context.Response.HasStarted)
                            WriteExceptionToResponse(context, ex);
                        else
                            throw;
                    }
                    finally
                    {
                        // Thread unsafe call!
                        debugController?.DetachFromThread();
                    }

                    return Task.CompletedTask;
                });
            });
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

        [ContextMethod("ИспользоватьВебСокеты", "UseWebSockets")]
        public void UseWebSockets() => _useWebSockets = true;

        [ContextMethod("ДобавитьОбработчикИсключений", "AddExceptionsHandler")]
        public void SetExceptionsHandler(IRuntimeContextInstance target, string methodName)
            => _exceptionHandler = (target, methodName);

        [ContextMethod("УстановитьКаталогСервера", "SetServerDir")]
        public void SetContentRoot(IValue path)
        {
            _contentRoot = path.AsString();
        }
        
        [ContextMethod("УстановитьКорневойПуть", "SetWebRoot")]
        public void SetWebRoot(IValue path)
        {
            _wwwRoot = path.AsString();
        }

        [ContextMethod("ИспользоватьСтатическиеФайлы", "UseStaticFiles")]
        public void UseStaticFiles()
        {
            _useStaticFiles = true;
        }
    }
}
