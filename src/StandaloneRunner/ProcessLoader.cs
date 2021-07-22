/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using OneScript.Commons;
using OneScript.Sources;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;

namespace StandaloneRunner
{
    public class ProcessLoader
    {
        public Process CreateProcess(Stream sourceStream, IHostApplication host)
        {
            var appDump = DeserializeAppDump(sourceStream);
            
            var engineBuilder = DefaultEngineBuilder
                .Create()
                .SetupEnvironment(e =>
                {
                    e.AddAssembly(typeof(ArrayImpl).Assembly);
                    e.UseTemplateFactory(new StandaloneTemplateFactory());
                })
                .SetupConfiguration(p => p.UseEnvironmentVariableConfig("OSCRIPT_CONFIG"));

            var engine = new HostedScriptEngine(engineBuilder.Build());
            var src = SourceCodeBuilder.Create()
                .FromSource(new BinaryCodeSource())
                .WithName("Compiler source")
                .Build();
            
            engine.SetGlobalEnvironment(host, src);
            engine.InitializationCallback = (e, env) =>
            {
                var storage = e.GlobalsManager.GetInstance<TemplateStorage>();
                LoadResources(storage, appDump.Resources);
            };
            engine.Initialize();

            LoadScripts(engine, appDump.Scripts);
            
            var process = engine.CreateProcess(host, appDump.Scripts[0].Image, src);

            return process;
        }

        private void LoadResources(TemplateStorage templateStorage, ApplicationResource[] resources)
        {
            foreach (var resource in resources)
            {
                templateStorage.RegisterTemplate(resource.ResourceName, DeserializeTemplate(resource.Data));
            }
        }

        private void LoadScripts(HostedScriptEngine engine, UserAddedScript[] scripts)
        {
            var binaryIndexer = new CompiledCodeIndexer();
            var module = scripts[0].Image;
            module.ModuleInfo.CodeIndexer = binaryIndexer;
            
            scripts
                .Skip(1)
                .Where(x => x.Type == UserAddedScriptType.Module)
                .OrderBy(x => x.InjectOrder)
                .ForEach(x =>
                {
                    x.Image.ModuleInfo.CodeIndexer = binaryIndexer;
                    engine.EngineInstance.Environment.InjectGlobalProperty(null, x.Symbol, true);
                });

            foreach (var userAddedScript in scripts.Skip(1).Where(x => x.Type == UserAddedScriptType.Class))
            {
                userAddedScript.Image.ModuleInfo.CodeIndexer = binaryIndexer;
                engine.LoadUserScript(userAddedScript);
            }
            
            scripts
                .Skip(1)
                .Where(x => x.Type == UserAddedScriptType.Module)
                .ForEach(x =>
            {
                var loaded = engine.EngineInstance.LoadModuleImage(x.Image);
                var instance = engine.EngineInstance.CreateUninitializedSDO(loaded);
                engine.EngineInstance.Environment.SetGlobalProperty(x.Symbol, instance);
                engine.EngineInstance.InitializeSDO(instance);
            });
        }

        private static ApplicationDump DeserializeAppDump(Stream sourceStream)
        {
            var serializer = new BinaryFormatter();
            var appDump = (ApplicationDump) serializer.Deserialize(sourceStream);
            return appDump;
        }

        private ITemplate DeserializeTemplate(byte[] resourceData)
        {
            byte[] templateBytes;
            TemplateKind kind;
            using (var ms = new MemoryStream(resourceData))
            {
                var br = new BinaryReader(ms);
                var tKind = br.ReadString();
                switch (tKind)
                {
                    case "File":
                        kind = TemplateKind.File;
                        break;
                    case "BinaryData":
                        kind = TemplateKind.BinaryData;
                        break;
                    default:
                        throw new Exception($"Unknown template kind {tKind}");
                }

                var len = br.ReadInt32();
                templateBytes = br.ReadBytes(len);
            }

            return new InternalTemplate(templateBytes, kind);
        }
    }
}