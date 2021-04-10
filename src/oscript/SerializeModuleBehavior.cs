/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using OneScript.Commons;
using ScriptEngine;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;

namespace oscript
{
    internal class SerializeModuleBehavior : AppBehavior
    {
        private readonly string _path;
        public SerializeModuleBehavior(string path)
        {
            _path = path;
        }

        public override int Execute()
        {
            var builder = ConsoleHostBuilder.Create(_path);
            var engine = ConsoleHostBuilder.Build(builder);
            
            engine.Initialize();
            ScriptFileHelper.OnBeforeScriptRead(engine);
            var source = engine.Loader.FromFile(_path);
            var compiler = engine.GetCompilerService();
            engine.SetGlobalEnvironment(new DoNothingHost(), source);
            var entry = compiler.Compile(source);
            var embeddedContext = engine.GetExternalLibraries();

            var serializer = new JsonSerializer();
            var sb = new StringBuilder();
            using (var textWriter = new StringWriter(sb))
            {
                var writer = new JsonTextWriter(textWriter);
                writer.WriteStartArray();
                
                WriteImage(new UserAddedScript
                {
                    Type = UserAddedScriptType.Module,
                    Symbol = "$entry",
                    Image = entry
                }, writer, serializer);

                foreach (var item in embeddedContext)
                {
                    item.Classes.ForEach(x => WriteImage(x, writer, serializer));
                    item.Modules.ForEach(x => WriteImage(x, writer, serializer));
                }

                writer.WriteEndArray();
            }

            string result = sb.ToString();
            Output.WriteLine(result);

            return 0;
        }

        private void WriteImage(UserAddedScript script, JsonTextWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteValue(script.Symbol);
            writer.WritePropertyName("type");
            writer.WriteValue(script.Type.ToString());
            writer.WritePropertyName("image");
            serializer.Serialize(writer, script.Image);
            writer.WriteEndObject();
        }

        public static AppBehavior Create(CmdLineHelper helper)
        {
            var path = helper.Next();
            if (path != null)
            {
                return new SerializeModuleBehavior(path);
            }

            return new ShowUsageBehavior();
        }
    }
}
