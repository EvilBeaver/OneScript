/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneScript.Execution;

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

            builder.Services.Configure<FormOptions>(builder.Configuration.GetSection("FormOptions"));

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

            _app.Use((context, next) =>
            {
                var process = _executionContext.Services.Resolve<IBslProcessFactory>().NewProcess();
                context.Items.Add(typeof(IBslProcess), process);
                return next();
            });

            _middlewares.ForEach(middleware =>
            {
                _app.Use((context, next) =>
                {
                    var args = new IValue[]
                    {
                        new HttpContextWrapper(_executionContext.TypeManager, context),
                        new RequestDelegateWrapper(next)
                    };

                    var process = (IBslProcess)context.Items[typeof(IBslProcess)];
                    
                    var methodNumber = middleware.Target.GetMethodNumber(middleware.MethodName);
                    middleware.Target.CallAsProcedure(methodNumber, args, process);

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

                    var methodNumber = _exceptionHandler?.Target.GetMethodNumber(_exceptionHandler?.MethodName)
                        ?? throw new InvalidOperationException();

                    var process = _executionContext.Services.Resolve<IBslProcessFactory>().NewProcess();

                    try
                    {
                        _exceptionHandler?.Target.CallAsProcedure(methodNumber, args, process);
                    }
                    catch (Exception ex)
                    {
                        if (!context.Response.HasStarted)
                            WriteExceptionToResponse(context, ex);
                        else
                            throw;
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
        public void SetContentRoot(string path)
        {
            _contentRoot = path;
        }
        
        [ContextMethod("УстановитьКорневойПуть", "SetWebRoot")]
        public void SetWebRoot(string path)
        {
            _wwwRoot = path;
        }

        [ContextMethod("ИспользоватьСтатическиеФайлы", "UseStaticFiles")]
        public void UseStaticFiles()
        {
            _useStaticFiles = true;
        }
    }
}
