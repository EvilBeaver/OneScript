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
        static List<System.Reflection.Assembly> _assembliesForAttaching;

        private static string _configFilePath;

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
            _configFilePath = appSettings["configFilePath"];

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
            System.Collections.Specialized.NameValueCollection appSettings = System.Web.Configuration.WebConfigurationManager.AppSettings;

            // Инициализируем логгирование, если надо
            string logPath = appSettings["logToPath"];
            StreamWriter logWriter = null;

            try
            {
                if (logPath != null)
                {
                    logPath = HttpContext.Current.Server.MapPath(logPath);
                    string logFileName = Guid.NewGuid().ToString().Replace("-", "") + ".txt";
                    logWriter = File.CreateText(Path.Combine(logPath, logFileName));
                }
            }
            catch { /*что-то не так, ничего не делаем. Возможно нет папки или прав на запись.*/}

            // Если определена секция logToPath и удалось создать/открыть файл, считаем, что логгирование включено

            WriteToLog(logWriter, "Start loading.");

            try
            {
                _hostedScript = new HostedScriptEngine();
                // Размещаем oscript.cfg вместе с web.config. Так наверное привычнее
                _hostedScript.CustomConfig = _configFilePath ?? HttpContext.Current.Server.MapPath("~/oscript.cfg");
                _hostedScript.Initialize();
                _hostedScript.AttachAssembly(System.Reflection.Assembly.GetExecutingAssembly());

                // Аттачим доп сборки. По идее должны лежать в Bin
                foreach (System.Reflection.Assembly assembly in _assembliesForAttaching)
                {
                    try
                    {
                        _hostedScript.AttachAssembly(assembly);
                    }
                    catch (Exception ex)
                    {
                        // Возникла проблема при аттаче сборки
                        WriteToLog(logWriter, "Assembly attaching error: " + ex.Message);
                    }
                }

                //Загружаем библиотечные скрипты aka общие модули


                string libPath = appSettings["commonModulesPath"];

                if (libPath != null)
                {
                    libPath = HttpContext.Current.Server.MapPath(libPath);

                    string[] files = System.IO.Directory.GetFiles(libPath, "*.os");


                    foreach (string filePathName in files)
                    {
                        _hostedScript.InjectGlobalProperty(System.IO.Path.GetFileNameWithoutExtension(filePathName), ValueFactory.Create(), true);
                    }

                    foreach (string filePathName in files)
                    {
                        try
                        {
                            ICodeSource src = _hostedScript.Loader.FromFile(filePathName);

                            var compilerService = _hostedScript.GetCompilerService();
                            var module = compilerService.CreateModule(src);
                            var loaded = _hostedScript.EngineInstance.LoadModuleImage(module);
                            var instance = (IValue)_hostedScript.EngineInstance.NewObject(loaded);
                            _hostedScript.EngineInstance.Environment.SetGlobalProperty(System.IO.Path.GetFileNameWithoutExtension(filePathName), instance);

                        }
                        catch (Exception ex)
                        {
                            // Возникла проблема при загрузке файла os, логгируем, если логгирование включено
                            WriteToLog(logWriter, "Error loading " + System.IO.Path.GetFileNameWithoutExtension(filePathName) + " : " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Возникла проблема при инициализации хэндлера
                WriteToLog(logWriter, ex.Message);
            }

            WriteToLog(logWriter, "End loading.");
            // Закрываем лог, если надо
            if (logWriter != null)
            {
                try
                {
                    logWriter.Flush();
                    logWriter.Close();
                }
                catch
                { /*что-то не так, ничего не делаем.*/ }
            }

        }

        void WriteToLog(TextWriter logWriter, string message)
        {
            if (logWriter == null)
                return;
            try
            {
                logWriter.WriteLine(message);
            }
            catch { /* что-то не так, ничего не делаем */ }
        }

        public void ProcessRequest(HttpContext context)
        {

            #region Загружаем скрипт (файл .os)
            // Кэшируем исходный файл, если файл изменился (изменили скрипт .os) загружаем заново
            // В Linux под Mono не работает подписка на изменение файла.
            LoadedModuleHandle? module = null;
            ObjectCache cache = MemoryCache.Default;

            if (_cachingEnabled)
                module = cache[context.Request.PhysicalPath] as LoadedModuleHandle?;

            if (module == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();

                List<string> filePaths = new List<string>();
                filePaths.Add(context.Request.PhysicalPath);
                policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

                // Загружаем файл и помещаем его в кэш
                module = LoadByteCode(context.Request.PhysicalPath);
                cache.Set(context.Request.PhysicalPath, module, policy);
            }
            #endregion

            var runner = CreateServiceInstance(module.Value);

            int exitCode = 0;

            try
            {
                int methodIndex = runner.FindMethod("ОбработкаВызоваHTTPСервиса");
                IValue result;
                IValue[] args = new IValue[1];
                args[0] = new ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceRequestImpl(context);

                runner.CallAsFunction(methodIndex, args, out result);

                // Обрабатываем результаты
                var response = (ScriptEngine.HostedScript.Library.HTTPService.HTTPServiceResponseImpl)result;
                context.Response.StatusCode = response.StatusCode;

                if (response.Headers != null)
                {

                    foreach (var ch in response.Headers)
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

        private IRuntimeContextInstance CreateServiceInstance(LoadedModuleHandle module)
        {
            var runner = _hostedScript.EngineInstance.NewObject(module);
            return runner;
        }

        private LoadedModuleHandle LoadByteCode(string filePath)
        {
            var code = _hostedScript.EngineInstance.Loader.FromFile(filePath);
            var compiler = _hostedScript.GetCompilerService();
            var byteCode = compiler.CreateModule(code);
            var module = _hostedScript.EngineInstance.LoadModuleImage(byteCode);
            return module;
        }
    }
}
