/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Linq;
using System.Threading.Tasks;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Execution;
using OneScript.Sources;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Compiler;
using ScriptEngine.Hosting;

namespace ScriptEngine
{
    public class ScriptingEngine : IDisposable
    {
        private AttachedScriptsFactory _attachedScriptsFactory;
        private IDebugController _debugController;

        public ScriptingEngine(ITypeManager types,
            IGlobalsManager globals,
            RuntimeEnvironment env, 
            ConfigurationProviders configurationProviders,
            IServiceContainer services)
        {
            TypeManager = types;
            // FIXME: Пока потребители не отказались от статических инстансов, они будут жить и здесь
            
            GlobalsManager = globals;
            Environment = env;
            
            Loader = new ScriptSourceFactory();
            Services = services;
            ContextDiscoverer = new ContextDiscoverer(types, globals, services);
            DebugController = services.TryResolve<IDebugController>();
            ApplyConfiguration(configurationProviders.CreateConfig());
        }

        public IServiceContainer Services { get; }

        private void ApplyConfiguration(KeyValueConfig config)
        {
            var options = new OneScriptCoreOptions(config);

            Loader.ReaderEncoding = options.FileReaderEncoding;
        }

        private ContextDiscoverer ContextDiscoverer { get; }
        
        public RuntimeEnvironment Environment { get; set; }

        public ITypeManager TypeManager { get; }
        
        public IGlobalsManager GlobalsManager { get; }
        
        private CodeGenerationFlags ProduceExtraCode { get; set; }
        
        public void AttachAssembly(System.Reflection.Assembly asm, Predicate<Type> filter = null)
        {
            ContextDiscoverer.DiscoverClasses(asm, filter);
            ContextDiscoverer.DiscoverGlobalContexts(Environment, asm, filter);
        }

        public void AttachExternalAssembly(System.Reflection.Assembly asm, RuntimeEnvironment globalEnvironment)
        {
            ContextDiscoverer.DiscoverClasses(asm);

            var lastCount = globalEnvironment.AttachedContexts.Count();
            ContextDiscoverer.DiscoverGlobalContexts(globalEnvironment, asm);

            var newCount = globalEnvironment.AttachedContexts.Count();
            while (lastCount < newCount)
            {
                MachineInstance.Current.AttachContext(globalEnvironment.AttachedContexts[lastCount].Instance);
                ++lastCount;
            }
        }
        
        public void AttachExternalAssembly(System.Reflection.Assembly asm)
        {
            AttachExternalAssembly(asm, Environment);
        }

        public void Initialize()
        {
            SetDefaultEnvironmentIfNeeded();

            UpdateContexts();

            _attachedScriptsFactory = new AttachedScriptsFactory(this);
            AttachedScriptsFactory.SetInstance(_attachedScriptsFactory);
        }

        public void UpdateContexts()
        {
            ExecutionDispatcher.Current ??= Services.Resolve<ExecutionDispatcher>();
            MachineInstance.Current.SetMemory(Services.Resolve<ExecutionContext>());
        }

        private void SetDefaultEnvironmentIfNeeded()
        {
            if (Environment == null)
                Environment = new RuntimeEnvironment();
        }

        public ScriptSourceFactory Loader { get; }

        public ICompilerService GetCompilerService()
        {
            using var scope = Services.CreateScope();
            var csFactory = scope.Resolve<ICompilerServiceFactory>();
            
            var cs = csFactory.CreateInstance(Environment.Symbols);
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    cs.DefinePreprocessorValue("Linux");
                    break;
                case PlatformID.MacOSX:
                    cs.DefinePreprocessorValue("MacOS");
                    break;
                case PlatformID.Win32NT:
                    cs.DefinePreprocessorValue("Windows");
                    break;
            }
            
            cs.GenerateDebugCode = ProduceExtraCode.HasFlag(CodeGenerationFlags.DebugCode);
            cs.GenerateCodeStat = ProduceExtraCode.HasFlag(CodeGenerationFlags.CodeStatistics);
            return cs;
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

        public StackRuntimeModule LoadModuleImage(ModuleImage moduleImage)
        {
            throw new NotImplementedException("Deserialization of module not implemented");
            //return new LoadedModule(moduleImage);
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

        public void SetCodeStatisticsCollector(ICodeStatCollector collector)
        {
            ProduceExtraCode |= CodeGenerationFlags.CodeStatistics;
            MachineInstance.Current.SetCodeStatisticsCollector(collector);
        }

        #region IDisposable Members

        public void Dispose()
        {
            AttachedScriptsFactory.SetInstance(null);
        }

        #endregion
    }
}
