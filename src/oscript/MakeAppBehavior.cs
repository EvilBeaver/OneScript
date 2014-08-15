using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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
            Console.WriteLine("Make started...");
            using (var exeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("oscript.StandaloneRunner.exe"))
            using (var output = new FileStream(_exePath, FileMode.Create))
            {
                exeStream.CopyTo(output);

                int offset = (int)output.Length;

                var engine = new ScriptEngine.HostedScriptEngine();
                engine.Initialize();
                var source = engine.Loader.FromFile(_codePath);
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var persistor = new ScriptEngine.Compiler.ModulePersistor(formatter);
                var compiler = engine.GetCompilerService();
                persistor.Save(compiler.CreateModule(source), output);

                byte[] signature = new byte[4]
                    {
                        0x4f,0x53,0x4d,0x44
                    };
                output.Write(signature, 0, signature.Length);
                using (var bw = new BinaryWriter(output))
                {
                    bw.Write(offset);
                }
            }
            Console.WriteLine("Make completed");
            return 0;
        }
    }

}
