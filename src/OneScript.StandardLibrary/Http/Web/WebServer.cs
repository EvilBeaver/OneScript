using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Http;
using OneScript.StandardLibrary.Tasks;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ExecutionContext = ScriptEngine.Machine.ExecutionContext;

namespace OneScript.StandardLibrary.Http.Web
{
    [ContextClass("ВебСервер", "WebServer")]
    public class WebServer: AutoContext<WebServer>, IDisposable
    {
        private readonly ExecutionContext _executionContext;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private WebApplication _app;
        private readonly List<(IRuntimeContextInstance Target, string MethodName)> _middlewares = new List<(IRuntimeContextInstance Target, string MethodName)>();
        private string _contentRoot = null;
        private bool _useStaticFiles = false;
        private bool disposedValue;

        [ContextProperty("Порт", "Port", CanWrite = false)]
        public int Port { get; private set; }

        public WebServer() 
        {
            _executionContext = MachineInstance.Current.Memory;
        }

        [ScriptConstructor(Name = "С портом по умолчанию - 8080")]
        public static WebServer Constructor()
        {
            var server = new WebServer
            {
                Port = 8080
            };

            return server;
        }

        [ScriptConstructor(Name = "С указанием порта прослушивателя")]
        public static WebServer Constructor(IValue port)
        {
            var server = new WebServer
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

                    MachineInstance.Current.SetMemory(_executionContext);

                    var debugController = _executionContext.Services.TryResolve<IDebugController>();
                    debugController?.AttachToThread();

                    try
                    {
                        middleware.Target.CallAsProcedure(methodNumber, args);
                    }
                    catch
                    {
                        debugController?.DetachFromThread();
                    }

                    return Task.CompletedTask;
                });
            });

            _app.RunAsync(_cancellationTokenSource.Token);
        }

        [ContextMethod("ДобавитьОбработчикЗапросов", "AddRequestsHandler")]
        public void SetRequestsHandler(IRuntimeContextInstance target, string methodName)
            => _middlewares.Add((target, methodName));

        [ContextMethod("ЖдатьОстановки", "WaitForShutdown")]
        public void WaitForShutdown()
        {
            _app?.WaitForShutdown();
        }

        [ContextMethod("Остановить", "Stop")]
        public void Stop()
        {
            _app?.StopAsync(_cancellationTokenSource.Token);
        }

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
