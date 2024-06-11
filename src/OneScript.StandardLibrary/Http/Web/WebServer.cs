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

namespace OneScript.StandardLibrary.Http.Web
{
    [ContextClass("ВебСервер", "WebServer")]
    public class WebServer: AutoContext<WebServer>, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private WebApplication _app;
        private List<(IRuntimeContextInstance Target, string MethodName)> _middlewares = new List<(IRuntimeContextInstance Target, string MethodName)>();
        private bool disposedValue;

        [ContextProperty("Порт", "Port", CanWrite = false)]
        public int Port { get; private set; }

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

            _app = builder.Build();

            var tasksManager = new BackgroundTasksManager(MachineInstance.Current.Memory);

            _middlewares.ForEach(middleware =>
            {
                _app.Use((context, next) =>
                {
                    var args = new ArrayImpl
                    {
                        new HttpContextWrapper(context),
                        new RequestDelegateWrapper(next)
                    };

                    var task = tasksManager.Execute(middleware.Target, middleware.MethodName, args);

                    return task.WorkerTask.ContinueWith(t =>
                    {
                        if (task.State == TaskStateEnum.CompletedWithErrors)
                        {
                            context.Response.StatusCode = 500;

                            var content = Encoding.UTF8.GetBytes(task.ExceptionInfo.MessageWithoutCodeFragment);
                            context.Response.Headers.ContentType = "text/plain;charset=utf-8";
                            context.Response.Headers.ContentLength = content.Length;
                            context.Response.Body.Write(content);
                        }
                    });
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
