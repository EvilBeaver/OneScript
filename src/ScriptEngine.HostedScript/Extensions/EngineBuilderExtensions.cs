/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.IO;
using ScriptEngine.Compiler;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Extensions
{
    public static class EngineBuilderExtensions
    {
        public static IEngineBuilder UseConfigFile(this IEngineBuilder b, string configFile)
        {
            if (System.IO.File.Exists(configFile))
            {
                var reader = new CfgFileConfigProvider
                {
                    FilePath = configFile
                };
                b.ConfigurationProviders.Add(reader.GetProvider());
            }

            return b;
        }

        public static IEngineBuilder UseSystemConfigFile(this IEngineBuilder b)
        {
            var asmLocation = typeof(IValue).Assembly.Location;
            if (string.IsNullOrEmpty(asmLocation))
                asmLocation = System.Reflection.Assembly.GetEntryAssembly()?.Location;

            var pathPrefix = !string.IsNullOrWhiteSpace(asmLocation) ? 
                System.IO.Path.GetDirectoryName(asmLocation) :
                System.Environment.CurrentDirectory;
            
            var configFile = System.IO.Path.Combine(pathPrefix, CfgFileConfigProvider.CONFIG_FILE_NAME);

            return b.UseConfigFile(configFile);
        }
        
        public static IEngineBuilder UseEntrypointConfigFile(this IEngineBuilder b, string entryPoint)
        {
            var dir = System.IO.Path.GetDirectoryName(entryPoint);
            var cfgPath = System.IO.Path.Combine(dir, CfgFileConfigProvider.CONFIG_FILE_NAME);
            if (System.IO.File.Exists(cfgPath))
            {
                return b.UseConfigFile(cfgPath); 
            }

            return b;
        }
        
        public static IEngineBuilder UseEnvironmentVariableConfig(this IEngineBuilder b, string varName)
        {
            var env = System.Environment.GetEnvironmentVariable(varName);
            if(env == null)
                return b;

            var reader = new FormatStringConfigProvider
            {
                ValuesString = env
            };
            
            b.ConfigurationProviders.Add(reader.GetProvider());
            return b;
        }
        
        public static IEngineBuilder UseFileSystemLibraries(this IEngineBuilder b)
        {
            var config = b.ConfigurationProviders.CreateConfig();

            var searchDirs = new List<string>();
            
            var sysDir = config[OneScriptOptions.SYSTEM_LIBRARY_DIR];
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
            
            var additionalDirsList = config[OneScriptOptions.ADDITIONAL_LIBRARIES];

            if (additionalDirsList != null)
            {
                var addDirs = additionalDirsList.Split(';');
                searchDirs.AddRange(addDirs);
            }

            b.CompilerOptions.UseFileSystemLibraries(searchDirs);
            return b;
        }
        
        public static CompilerOptions UseFileSystemLibraries(this CompilerOptions b, IEnumerable<string> searchDirectories)
        {
            var resolver = new FileSystemDependencyResolver();
            resolver.SearchDirectories.AddRange(searchDirectories);
            return b.UseImports(resolver);
        }
    }
}