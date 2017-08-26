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
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ScriptEngine.HostedScript;
using ScriptEngine;

namespace oscript
{
    class MakeAppBehavior : AppBehavior 
    {
        string _codePath;
        string _exePath;

        public MakeAppBehavior(string codePath, string exePath)
        {
            _codePath = codePath;
            _exePath = exePath;
        }

        public override int Execute()
        {
            Output.WriteLine("Make started...");
            using (var exeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("oscript.StandaloneRunner.exe"))
            using (var output = new FileStream(_exePath, FileMode.Create))
            {
                exeStream.CopyTo(output);

                int offset = (int)output.Length;

                var engine = new HostedScriptEngine();
                engine.CustomConfig = ScriptFileHelper.CustomConfigPath(_codePath);
                engine.Initialize();
                ScriptFileHelper.OnBeforeScriptRead(engine);
                var source = engine.Loader.FromFile(_codePath);
                var compiler = engine.GetCompilerService();
                engine.SetGlobalEnvironment(new DoNothingHost(), source);
                var entry = compiler.CreateModule(source);

                var embeddedContext = engine.GetUserAddedScripts();

                using (var bw = new BinaryWriter(output))
                {
                    bw.Write(embeddedContext.Count() + 1);

                    var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    var persistor = new ScriptEngine.Compiler.ModulePersistor(formatter);
                    persistor.Save(new UserAddedScript()
                        {
                            Type = UserAddedScriptType.Module,
                            Symbol = "$entry",
                            Module = entry
                        }, output);

                    foreach (var item in embeddedContext)
                    {
                        persistor.Save(item, output);
                    }

                    byte[] signature = new byte[4]
                    {
                        0x4f,0x53,0x4d,0x44
                    };
                    output.Write(signature, 0, signature.Length);

                    bw.Write(offset);
                }
            }
            Output.WriteLine("Make completed");
            return 0;
        }
    }
}
