/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using System.Threading.Tasks;
using OneScript.Compilation;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Compiler;
using ScriptEngine.Libraries;

namespace ScriptEngine
{
    public class ScriptingEngine : IDisposable
    {
        private AttachedScriptsFactory _attachedScriptsFactory;
        private IDebugController _debugController;
        private IRuntimeEnvironment _runtimeEnvironment;
        
        private readonly ILibraryManager _libraryManager;

        public ScriptingEngine(ITypeManager types,
            IGlobalsManager globals,
            RuntimeEnvironment env, 
            OneScriptCoreOptions options,
            IServiceContainer services)
        {
            TypeManager = types;
            // FIXME: Пока потребители не отказались от статических инстансов, они будут жить и здесь
            
            GlobalsManager = globals;
            _runtimeEnvironment = env;
            _libraryManager = env;
            
            Loader = new ScriptSourceFactory();
            Services = services;
            ContextDiscoverer = new ContextDiscoverer(types, globals, services);
            DebugController = services.TryResolve<IDebugController>();
            Loader.ReaderEncoding = options.FileReaderEncoding;
        }

        public IServiceContainer Services { get; }

        private ContextDiscoverer ContextDiscoverer { get; }

        public IRuntimeEnvironment Environment => _runtimeEnvironment;

        public ILibraryManager LibraryManager => _libraryManager;

        public ITypeManager TypeManager { get; }
        
        public IGlobalsManager GlobalsManager { get; }
        
        private CodeGenerationFlags ProduceExtraCode { get; set; }
        
        public void AttachAssembly(System.Reflection.Assembly asm, Predicate<Type> filter = null)
        {
            ContextDiscoverer.DiscoverClasses(asm, filter);
            ContextDiscoverer.DiscoverGlobalContexts(Environment, asm, filter);
        }

        public void AttachExternalAssembly(System.Reflection.Assembly asm, IRuntimeEnvironment globalEnvironment)
        {
            ContextDiscoverer.DiscoverClasses(asm);

            //var lastCount = globalEnvironment.AttachedContexts.Count();
            ContextDiscoverer.DiscoverGlobalContexts(globalEnvironment, asm);

            //var newCount = globalEnvironment.AttachedContexts.Count();
            // while (lastCount < newCount)
            // {
            //     MachineInstance.Current.AttachContext(globalEnvironment.AttachedContexts[lastCount]);
            //     ++lastCount;
            // }
        }
        
        public void AttachExternalAssembly(System.Reflection.Assembly asm)
        {
            AttachExternalAssembly(asm, Environment);
        }

        public void Initialize()
        {
            SetDefaultEnvironmentIfNeeded();
            EnableCodeStatistics();
            UpdateContexts();

            _attachedScriptsFactory = new AttachedScriptsFactory(this);
            AttachedScriptsFactory.SetInstance(_attachedScriptsFactory);
        }

        public void UpdateContexts()
        {
            lock (this)
            {
                ExecutionDispatcher.Current ??= Services.Resolve<ExecutionDispatcher>();
            }
            MachineInstance.Current.SetMemory(Services.Resolve<ExecutionContext>());
        }

        private void SetDefaultEnvironmentIfNeeded()
        {
            _runtimeEnvironment ??= new RuntimeEnvironment();
        }

        public ScriptSourceFactory Loader { get; }

        public ICompilerFrontend GetCompilerService()
        {
            using var scope = Services.CreateScope();
            var compiler = scope.Resolve<CompilerFrontend>();
            compiler.SharedSymbols = _runtimeEnvironment.GetSymbolTable();
            
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    compiler.PreprocessorDefinitions.Add("Linux");
                    break;
                case PlatformID.MacOSX:
                    compiler.PreprocessorDefinitions.Add("MacOS");
                    break;
                case PlatformID.Win32NT:
                    compiler.PreprocessorDefinitions.Add("Windows");
                    break;
            }
            
            compiler.GenerateDebugCode = ProduceExtraCode.HasFlag(CodeGenerationFlags.DebugCode);
            compiler.GenerateCodeStat = ProduceExtraCode.HasFlag(CodeGenerationFlags.CodeStatistics);
            return compiler;
        }
        
        public IRuntimeContextInstance NewObject(IExecutableModule module, ExternalContextData externalContext = null)
        {
            var scriptContext = CreateUninitializedSDO(module, externalContext);
            InitializeSDO(scriptContext);

            return scriptContext;
        }

        public ScriptDrivenObject CreateUninitializedSDO(IExecutableModule module, ExternalContextData externalContext = null)
        {
            var scriptContext = new UserScriptContextInstance(module, true);
            if (externalContext != null)
            {
                foreach (var item in externalContext)
                {
                    scriptContext.AddProperty(item.Key, item.Value);
                }
            }

            scriptContext.InitOwnData();
            return scriptContext;
        }

        public void InitializeSDO(ScriptDrivenObject sdo)
        {
            sdo.Initialize();
        }
        
        public Task InitializeSDOAsync(ScriptDrivenObject sdo)
        {
            return sdo.InitializeAsync();
        }

        public AttachedScriptsFactory AttachedScriptsFactory => _attachedScriptsFactory;

        public IDebugController DebugController
        {
            get => _debugController;
            private set
            {
                _debugController = value;
                if (value != null)
                {
                    ProduceExtraCode |= CodeGenerationFlags.DebugCode;
                    MachineInstance.Current.SetDebugMode(_debugController.BreakpointManager);
                }
            }
        }

        private void EnableCodeStatistics()
        {
            var collector = Services.TryResolve<ICodeStatCollector>();
            if (collector == default)
                return;
            
            ProduceExtraCode |= CodeGenerationFlags.CodeStatistics;
        }

        #region IDisposable Members

        public void Dispose()
        {
            DebugController?.Dispose();
            AttachedScriptsFactory.SetInstance(null);
        }

        #endregion
    }
}
