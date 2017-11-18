﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Runtime.Caching;
using System.IO;

using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;

namespace OneScript.ASPNETHandler
{
    public class ASPNETHandler : IHttpHandler
    {
        HostedScriptEngine hostedScript;
        ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceContext onescript_context;

        public bool IsReusable
        {
            // Разрешаем повторное использование и храним среду выполнения и контекст 
            get { return true; }
        }

        public ASPNETHandler()
        {
            hostedScript = new HostedScriptEngine();
            hostedScript.Initialize();
            onescript_context = new ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceContext();
        }

        public void ProcessRequest(HttpContext context)
        {
            // Устанавливаем указатель контекста в свойстве HTTPСервисКонтекст на текущий контекст вызова
            onescript_context.SetHTTPContext(context);

            #region Загружаем скрипт (файл .os)
            // Кэшируем исходный файл, если файл изменился (изменили скрипт .os) загружаем заново
            // Как это сделать с откомпилированным кодом, чтобы не компилировать?
            // В Linux под Mono не работает подписка на изменение файла.
            ObjectCache cache = MemoryCache.Default;
            string source_code_str = cache[context.Request.PhysicalPath] as string;

            if (source_code_str == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();

                List<string> filePaths = new List<string>();
                filePaths.Add(context.Request.PhysicalPath);
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

                // Загружаем файл и помещаем его в кэш
                source_code_str = File.ReadAllText(context.Request.PhysicalPath);
                cache.Set(context.Request.PhysicalPath, source_code_str, policy);
            }
            #endregion
            // Эта строка ЗОЛОТАЯ. Ее написание заняло 90% моего времени
            var runner = hostedScript.EngineInstance.AttachedScriptsFactory.LoadFromString(
                hostedScript.EngineInstance.GetCompilerService(), source_code_str);

            int exitCode = 0;

            try
            {
                int methodIndex = runner.FindMethod("ОбработкаВызоваHTTPСервиса");
                IValue result;
                IValue[] args = new IValue[1];
                args[0] = onescript_context;
                runner.CallAsFunction(methodIndex, args, out result);

                // Обрабатываем результаты
                ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceResponseImpl response = (ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceResponseImpl)result;
                context.Response.StatusCode = response.StatusCode;

                if (response.Headers != null)
                {

                    foreach (ScriptEngine.HostedScript.Library.KeyAndValueImpl ch in response.Headers)
                    {
                        context.Response.Headers.Add(ch.Key.AsString(), ch.Value.AsString());
                    }
                }

                if (response.Reason != "")
                {
                    context.Response.Status = response.Reason;
                }

                if (response.Body != null)
                {
                    context.Response.OutputStream.Write(response.Body, 0, response.Body.Length);
                }

            }
            catch (ScriptInterruptionException e)
            {
                exitCode = e.ExitCode;
                context.Response.StatusCode = 500;
                context.Response.Status = "Script running error";
                context.Response.SubStatusCode = exitCode;
                context.Response.StatusDescription = e.Message;
            }
            catch (Exception e)
            {
                context.Response.StatusCode = 500;
                context.Response.Status = e.Message;
            }
            finally
            {
                context.Response.End();
            }

        }
    }
}
