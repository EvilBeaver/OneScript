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

using ScriptEngine;
using ScriptEngine.HostedScript;

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
            var engine = new HostedScriptEngine
            {
                CustomConfig = ScriptFileHelper.CustomConfigPath(_path)
            };
            engine.Initialize();
            ScriptFileHelper.OnBeforeScriptRead(engine);
            var source = engine.Loader.FromFile(_path);
            var compiler = engine.GetCompilerService();
            engine.SetGlobalEnvironment(new DoNothingHost(), source);
            var entry = compiler.Compile(source);
            var embeddedContext = engine.GetUserAddedScripts();

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

                var userAddedScripts = embeddedContext as IList<UserAddedScript> ?? embeddedContext.ToList();
                foreach (var item in userAddedScripts)
                    WriteImage(item, writer, serializer);

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
    }
}
