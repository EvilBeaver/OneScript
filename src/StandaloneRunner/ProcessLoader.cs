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
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace StandaloneRunner
{
    public class ProcessLoader
    {
        public Process CreateProcess(Stream sourceStream, IHostApplication host)
        {
            var engine = new HostedScriptEngine();
            var src = new BinaryCodeSource();
            engine.SetGlobalEnvironment(host, src);

            var templateStorage = new TemplateStorage(new StandaloneTemplateFactory());
            engine.InitializationCallback = (e, env) =>
            {
                e.Environment.InjectObject(templateStorage);
                GlobalsManager.RegisterInstance(templateStorage);
            };

            engine.Initialize();

            var appDump = DeserializeAppDump(sourceStream);
            foreach (var resource in appDump.Resources)
            {
                templateStorage.RegisterTemplate(resource.ResourceName, DeserializeTemplate(resource.Data));
            }

            var module = appDump.Scripts[0].Image;

            var binaryIndexer = new CompiledCodeIndexer();
            module.ModuleInfo.CodeIndexer = binaryIndexer;

            var globalEnv = engine.EngineInstance.Environment;
            
            var loadedModules = new ScriptDrivenObject[appDump.Scripts.Length-1];
            for (int i = 1; i < appDump.Scripts.Length; i++)
            {
                var userAddedScript = appDump.Scripts[i];
                userAddedScript.Image.ModuleInfo.CodeIndexer = binaryIndexer;
                if (userAddedScript.Type == UserAddedScriptType.Class)
                {
                    engine.LoadUserScript(userAddedScript);
                }
                else
                {
                    var loaded = engine.EngineInstance.LoadModuleImage(userAddedScript.Image);
                    var instance = engine.EngineInstance.CreateUninitializedSDO(loaded);
                    globalEnv.InjectGlobalProperty(instance, userAddedScript.Symbol, true);
                    loadedModules[i-1] = instance;
                }
            }

            foreach (var instance in loadedModules.Where(x => x != null))
            {
                engine.EngineInstance.InitializeSDO(instance);
            }

            var process = engine.CreateProcess(host, module, src);

            return process;
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