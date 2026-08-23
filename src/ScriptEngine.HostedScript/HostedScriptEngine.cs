/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Sources;
using ScriptEngine.Machine;
using OneScript.Commons;
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Tasks;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript
{
    public class HostedScriptEngine : IDisposable
    {
        private readonly ScriptingEngine _engine;
        private SystemGlobalContext _globalCtx;
        private readonly IRuntimeEnvironment _env;
        private bool _isInitialized;

        private readonly OneScriptLibraryOptions _workingConfig;

        public HostedScriptEngine(ScriptingEngine engine)
        {
            _engine = engine;
            _env = _engine.Environment;
            _engine.AttachAssembly(typeof(HostedScriptEngine).Assembly);
            _workingConfig = _engine.Services.Resolve<OneScriptLibraryOptions>();
            SetGlobalContexts(engine.GlobalsManager);
        }

        public ScriptingEngine Engine => _engine;

        private void SetGlobalContexts(IGlobalsManager manager)
        {
            _globalCtx = new SystemGlobalContext();
            _globalCtx.EngineInstance = _engine;

            _env.InjectObject(_globalCtx);
            manager.RegisterInstance(_globalCtx);

            var dynLoader = new DynamicLoadingFunctions(_engine);
            _env.InjectObject(dynLoader);
            manager.RegisterInstance(dynLoader);

            var bgTasksManager = new BackgroundTasksManager(_engine.Services.Resolve<ExecutionContext>());
            _env.InjectGlobalProperty(bgTasksManager, "ФоновыеЗадания", "BackgroundJobs", true);
        }

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _engine.Initialize();
                _isInitialized = true;
            }

            // System language
            var systemLanguageCfg = _workingConfig.SystemLanguage;

            Locale.SystemLanguageISOName = systemLanguageCfg ?? System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        public void InjectGlobalProperty(string name, string alias, IValue value, bool readOnly)
        {
            _env.InjectGlobalProperty(value, name, alias, readOnly);
        }

        public void InjectObject(IAttachableContext obj)
        {
            _env.InjectObject(obj);
        }

        public ScriptSourceFactory Loader => _engine.Loader;

        public ICompilerFrontend GetCompilerService()
        {
            var compilerSvc = _engine.GetCompilerService();
            compilerSvc.FillSymbols(typeof(UserScriptContextInstance));
            DefineConstants(compilerSvc);
            return compilerSvc;
        }

        /// <summary>
        /// Создаёт процесс выполнения скрипта: инициализация, компиляция исходника, подготовка к запуску.
        /// </summary>
        /// <param name="host">Хост-приложение для взаимодействия со скриптом.</param>
        /// <param name="src">Исходный код скрипта.</param>
        /// <returns>Процесс, готовый к вызову <see cref="Process.Start"/>.</returns>
        /// <remarks>
        /// При ошибке компиляции или подготовки исключение пробрасывается вызывающему коду.
        /// Уведомление отладчика о завершении процесса выполняет вызывающий код
        /// (например, <see cref="RunProcess"/>).
        /// </remarks>
        public Process CreateProcess(IHostApplication host, SourceCode src)
        {
            Initialize();
            SetGlobalEnvironment(host, src);
            
            if (_engine.Debugger.IsEnabled)
            {
                _engine.Debugger.Start();
                _engine.Debugger.GetSession().WaitReadyToRun();
            }

            var compilerSvc = GetCompilerService();
            return Process.Create(_engine, compilerSvc, src);
        }

        /// <summary>
        /// Создаёт и запускает процесс скрипта, возвращает код завершения.
        /// </summary>
        /// <param name="host">Хост-приложение для взаимодействия со скриптом.</param>
        /// <param name="source">Исходный код скрипта.</param>
        /// <returns>
        /// Код завершения скрипта; при ошибке создания/выполнения — <c>1</c>
        /// после вывода информации об исключении через <see cref="IHostApplication.ShowExceptionInfo"/>.
        /// </returns>
        /// <remarks>
        /// Уведомляет отладчик о завершении процесса как при успешном, так и при аварийном исходе.
        /// Прерывание скрипта (<see cref="ScriptInterruptionException"/>) обрабатывается в <see cref="Process.Start"/>
        /// и возвращается как штатный код выхода.
        /// </remarks>
        public int RunProcess(IHostApplication host, SourceCode source)
        {
            try
            {
                var process = CreateProcess(host, source);
                var exitCode = process.Start();
                _engine.Debugger.NotifyProcessExit(exitCode);
                return exitCode;
            }
            catch (Exception e)
            {
                _engine.Debugger.NotifyProcessExit(1);
                host.ShowExceptionInfo(e);
                return 1;
            }
        }

        private void DefineConstants(ICompilerFrontend compilerSvc)
        {
            var definitions = _workingConfig.PreprocessorDefinitions;
            foreach (var val in definitions)
            {
                compilerSvc.PreprocessorDefinitions.Add(val);
            }

            if (Utils.IsMonoRuntime)
            {
                compilerSvc.PreprocessorDefinitions.Add("MONO");
            }
        }

        public IServiceContainer Services => _engine.Services;

        public void SetGlobalEnvironment(IHostApplication host, SourceCode src)
        {
            _globalCtx.ApplicationHost = host;
            _globalCtx.CodeSource = src;
            _globalCtx.InitInstance();
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
