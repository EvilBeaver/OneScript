using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Library;

namespace ScriptEngine
{
    public class Process
    {
        private IHostApplication _hostApp;
        private ICodeSource _source;
        private MachineInstance _machine;
        private List<InjectedObject> _injectedObjects = new List<InjectedObject>();

        private struct InjectedObject
        {
            public string Identifier;
            public IRuntimeContextInstance Instance;
        }

        private Process(IHostApplication host, ICodeSource codeSource)
        {
            _hostApp = host;
            _source = codeSource;
        }

        public void InjectObject(string name, object instance)
        {
            if (Machine.Library.DispatchUtility.ImplementsIDispatch(instance))
            {
                throw new ArgumentException("Instance must implement IDispatch");
            }

            if (!Utils.IsValidIdentifier(name))
            {
                throw new ArgumentException("Invalid name");
            }

            _injectedObjects.Add(new InjectedObject()
            {
                Identifier = name,
                Instance = new COMWrapperContext(instance)
            });

        }

        public int Start()
        {
            var globalCtx = new GlobalContext();
            RegisterGlobalProperties(globalCtx);
            globalCtx.SetProcess(this);
            _machine = new MachineInstance();
            _machine.AttachContext(globalCtx, false);
            ScriptSourceFactory.SetProvider(globalCtx);
            AttachedScriptsFactory.Init(_machine);
            try
            {
                var mainModule = new LoadedModule(_source.CreateModule().Module);
                var mainObj = new UserScriptContextInstance(mainModule);

                _machine.AttachContext(mainObj, true);
                _machine.SetModule(mainModule);

                _machine.Run();
                return 0;
            }
            catch (ScriptInterruptionException e)
            {
                return e.ExitCode;
            }
            catch (Exception e)
            {
                _hostApp.ShowExceptionInfo(e);
                return 1;
            }
            finally
            {
                AttachedScriptsFactory.Dispose();
            }
            
        }

        private void RegisterGlobalProperties(GlobalContext globalCtx)
        {
            globalCtx.RegisterProperty("АргументыКоманднойСтроки",new CommandLineArguments(ApplicationHost.GetCommandLineArguments()));
            foreach (var item in _injectedObjects)
            {
                globalCtx.RegisterProperty(item.Identifier, (IValue)item.Instance);
            }
        }

        internal IHostApplication ApplicationHost
        {
            get { return _hostApp; }
        }

        internal ICodeSource CodeSource
        {
            get
            {
                return _source;
            }
        }

        public static Process Create(IHostApplication host, ICodeSource scriptSource)
        {
            return new Process(host, scriptSource);
        }

    }

}
