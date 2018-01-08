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
    public class ASPNETHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        HostedScriptEngine _hostedScript;
        // Разрешает или запрещает кэширование исходников *.os В Linux должно быть false иначе после изменений исходника старая версия будет в кэше
        // web.config -> <appSettings> -> <add key="CachingEnabled" value="true"/>
        static bool _cachingEnabled;
        // Список дополнительных сборок, которые надо приаттачить к движку. Могут быть разные расширения
        // web.config -> <appSettings> -> <add key="ASPNetHandler" value="attachAssembly"/> Сделано так для простоты. Меньше настроек - дольше жизнь :)
        static System.Collections.Generic.List<System.Reflection.Assembly> _assembliesForAttaching;

        public bool IsReusable
        {
            // Разрешаем повторное использование и храним среду выполнения и контекст 
            get { return true; }
        }
        static ASPNETHandler()
        {
            _assembliesForAttaching = new List<System.Reflection.Assembly>();

            System.Collections.Specialized.NameValueCollection appSettings = System.Web.Configuration.WebConfigurationManager.AppSettings;
                
            _cachingEnabled = (appSettings["cachingEnabled"] == "true");

            foreach (string assemblyName in appSettings.AllKeys)
            {
                if (appSettings[assemblyName] == "attachAssembly")
                {
                    try
                    {
                        _assembliesForAttaching.Add(System.Reflection.Assembly.Load(assemblyName));
                    }
                    catch {/*не загрузилась, ничего не делаем*/ }
                }
            }
        }

        public ASPNETHandler()
        {
            _hostedScript = new HostedScriptEngine();
            _hostedScript.Initialize();
            _hostedScript.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            // Аттачим доп сборки. По идее должны лежать в Bin
            foreach (System.Reflection.Assembly assembly in _assembliesForAttaching)
            {
                try
                {
                    _hostedScript.AttachAssembly(assembly);
                }
                catch { /*что-то не так, ничего не делаем*/}
            }

            //Загружаем библиотечные скрипты aka общие модули
            System.Collections.Specialized.NameValueCollection appSettings = System.Web.Configuration.WebConfigurationManager.AppSettings;
            string libPath = appSettings["commonModulesPath"];

            if (libPath != null)
            {
                int a = 1;
            }
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
                        context.Response.AddHeader(ch.Key.AsString(), ch.Value.AsString());
                    }
                }

                if (response.Reason != "")
                {
                    context.Response.Status = response.Reason;
                }

                if (response.BodyStream != null)
                {
                    response.BodyStream.Seek(0, SeekOrigin.Begin);
                    response.BodyStream.CopyTo(context.Response.OutputStream);
                }

                context.Response.Charset = response.ContentCharset;

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
