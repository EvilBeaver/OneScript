/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

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
            
            var configFile = System.IO.Path.Combine(pathPrefix, EngineConfigProvider.CONFIG_FILE_NAME);

            return b.UseConfigFile(configFile);
        }
        
        public static IEngineBuilder UseEntrypointConfigFile(this IEngineBuilder b, string entryPoint)
        {
            var dir = System.IO.Path.GetDirectoryName(entryPoint);
            var cfgPath = System.IO.Path.Combine(dir, EngineConfigProvider.CONFIG_FILE_NAME);
            if (System.IO.File.Exists(cfgPath))
            {
                return b.UseConfigFile(cfgPath); 
            }

            return b;
        }
        
        public static IEngineBuilder UseEnvironmentVariable(this IEngineBuilder b, string varName)
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
    }
}