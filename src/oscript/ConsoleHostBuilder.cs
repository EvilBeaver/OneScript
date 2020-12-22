/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.HostedScript.Extensions;

namespace oscript
{
    internal static class ConsoleHostBuilder
    {
        public static IEngineBuilder Create()
        {
            var builder = new DefaultEngineBuilder();
            builder.SetDefaultOptions()
                .AddAssembly(typeof(ArrayImpl).Assembly)
                .UseSystemConfigFile()
                .UseEnvironmentVariableConfig("OSCRIPT_CONFIG");
            
            return builder;
        }

        public static HostedScriptEngine Build(IEngineBuilder builder)
        {
            var engine = builder.Build(); 
            var config = builder.ConfigurationProviders.CreateConfig();
            var openerEncoding = config["encoding.script"];
            
            if (!string.IsNullOrWhiteSpace(openerEncoding))
            {    
                if (StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") == 0)
                    engine.Loader.ReaderEncoding = FileOpener.SystemSpecificEncoding();
                else
                    engine.Loader.ReaderEncoding = Encoding.GetEncoding(openerEncoding);
            }

            return new HostedScriptEngine(engine)
            {
                Configuration = builder.ConfigurationProviders
            };
        }
    }
}