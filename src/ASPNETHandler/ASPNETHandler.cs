/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
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
        HostedScriptEngine _hostedScript;
        static bool _cachingEnabled; 

        public bool IsReusable
        {
            // Разрешаем повторное использование и храним среду выполнения и контекст 
            get { return true; }
        }
        static ASPNETHandler()
        {
            _cachingEnabled = (System.Web.Configuration.WebConfigurationManager.AppSettings["CachingEnabled"] == "true");
        }
        public ASPNETHandler()
        {
            _hostedScript = new HostedScriptEngine();
            _hostedScript.Initialize();
        }

        public void ProcessRequest(HttpContext context)
        {

            #region Загружаем скрипт (файл .os)
            // Кэшируем исходный файл, если файл изменился (изменили скрипт .os) загружаем заново
            // Как это сделать с откомпилированным кодом, чтобы не компилировать?
            // В Linux под Mono не работает подписка на изменение файла.
            string sourceCode = null;
            ObjectCache cache = MemoryCache.Default;

            if (_cachingEnabled)
                sourceCode = cache[context.Request.PhysicalPath] as string;

            if (sourceCode == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();

                List<string> filePaths = new List<string>();
                filePaths.Add(context.Request.PhysicalPath);
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

                // Загружаем файл и помещаем его в кэш
                sourceCode = File.ReadAllText(context.Request.PhysicalPath);
                cache.Set(context.Request.PhysicalPath, sourceCode, policy);
            }
            #endregion
            // Эта строка ЗОЛОТАЯ. Ее написание заняло 90% моего времени
            var runner = _hostedScript.EngineInstance.AttachedScriptsFactory.LoadFromString(
                _hostedScript.EngineInstance.GetCompilerService(), sourceCode);

            int exitCode = 0;

            try
            {
                int methodIndex = runner.FindMethod("ОбработкаВызоваHTTPСервиса");
                IValue result;
                IValue[] args = new IValue[1];
                args[0] = new ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceRequestImpl(context);

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
