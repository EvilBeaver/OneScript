/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using OneScript.Native.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Extensions
{
    public static class EngineBuilderExtensions
    {
        public static ConfigurationProviders UseConfigFile(this ConfigurationProviders providers, string configFile)
        {
            if (System.IO.File.Exists(configFile))
            {
                var reader = new CfgFileConfigProvider
                {
                    FilePath = configFile
                };
                providers.Add(reader.GetProvider());
            }

            return providers;
        }

        public static ConfigurationProviders UseSystemConfigFile(this ConfigurationProviders providers)
        {
            var asmLocation = typeof(IValue).Assembly.Location;
            if (string.IsNullOrEmpty(asmLocation))
                asmLocation = System.Reflection.Assembly.GetEntryAssembly()?.Location;

            var pathPrefix = !string.IsNullOrWhiteSpace(asmLocation) ? 
                System.IO.Path.GetDirectoryName(asmLocation) :
                System.Environment.CurrentDirectory;
            
            var configFile = System.IO.Path.Combine(pathPrefix, CfgFileConfigProvider.CONFIG_FILE_NAME);

            return providers.UseConfigFile(configFile);
        }
        
        public static ConfigurationProviders UseEntrypointConfigFile(this ConfigurationProviders providers, string entryPoint)
        {
            var dir = System.IO.Path.GetDirectoryName(entryPoint);
            var cfgPath = System.IO.Path.Combine(dir, CfgFileConfigProvider.CONFIG_FILE_NAME);
            if (System.IO.File.Exists(cfgPath))
            {
                return providers.UseConfigFile(cfgPath); 
            }

            return providers;
        }
        
        public static ConfigurationProviders UseEnvironmentVariableConfig(this ConfigurationProviders providers, string varName)
        {
            var env = System.Environment.GetEnvironmentVariable(varName);
            if(env == null)
                return providers;

            var reader = new FormatStringConfigProvider
            {
                ValuesString = env
            };
            
            providers.Add(reader.GetProvider());
            return providers;
        }
        
        public static IEngineBuilder UseFileSystemLibraries(this IEngineBuilder b)
        {
            b.Services.RegisterSingleton<IDependencyResolver>(sp =>
            {
                var config = sp.Resolve<KeyValueConfig>();

                var libOptions = new OneScriptLibraryOptions(config);
                var searchDirs = new List<string>();

                var sysDir = libOptions.SystemLibraryDir; 
                if (sysDir == default)
                {
                    var entrypoint = System.Reflection.Assembly.GetEntryAssembly();
                    if (entrypoint == default)
                        entrypoint = typeof(FileSystemDependencyResolver).Assembly;
                
                    sysDir = Path.GetDirectoryName(entrypoint.Location);
                    searchDirs.Add(sysDir);
                }
                else
                {
                    searchDirs.Add(sysDir);
                }
            
                if (libOptions.AdditionalLibraries != null)
                {
                    searchDirs.AddRange(libOptions.AdditionalLibraries);
                }
                
                var resolver = new FileSystemDependencyResolver();
                resolver.SearchDirectories.AddRange(searchDirs);

                return resolver;
            });
            
            return b;
        }

        public static MachineEnvironment UseTemplateFactory(this MachineEnvironment env, ITemplateFactory factory)
        {
            var storage = new TemplateStorage(factory);
            env.GlobalNamespace.InjectObject(storage);
            env.GlobalInstances.RegisterInstance(storage);
            return env;
        }

        public static IEngineBuilder UseNativeRuntime(this IEngineBuilder builder)
        {
            builder.Services.UseNativeRuntime();
            return builder;
        }
    }
}