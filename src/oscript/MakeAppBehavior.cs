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

using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Machine;

namespace oscript
{
	internal class MakeAppBehavior : AppBehavior
	{
		private readonly string _codePath;

		private readonly string _exePath;

		public MakeAppBehavior(string codePath, string exePath)
		{
			_codePath = codePath;
			_exePath = exePath;
		}

	    public bool CreateDumpOnly { get; set; }

	    public override int Execute()
		{
			Output.WriteLine("Make started...");

		    if (CreateDumpOnly)
		    {
		        using (var ms = new MemoryStream())
		        {
		            CreateDump(ms);
                }
		    }
		    else
		    {
		        CreateExe();
		    }

		    Output.WriteLine("Make completed");
			return 0;
		}

	    private void CreateDump(Stream output)
	    {
	        var offset = (int)output.Length;
	        
	        var builder = ConsoleHostBuilder.Create(_codePath);
	        builder.UseEntrypointConfigFile(_codePath);
	        var engine = ConsoleHostBuilder.Build(builder);
	        engine.Initialize();
	        ScriptFileHelper.OnBeforeScriptRead(engine);
	        var source = engine.Loader.FromFile(_codePath);
	        var compiler = engine.GetCompilerService();
	        engine.SetGlobalEnvironment(new DoNothingHost(), source);
	        var entry = compiler.Compile(source);

	        var embeddedContext = engine.GetExternalLibraries()
		        .SelectMany(x => x.Modules.Concat(x.Classes));
	        
	        var templates = GlobalsManager.GetGlobalContext<TemplateStorage>();

	        var dump = new ApplicationDump();
	        dump.Scripts = new UserAddedScript[]
	        {
		        new UserAddedScript()
		        {
			        Image = entry,
			        Symbol = "$entry",
			        Type = UserAddedScriptType.Module
		        }
	        }.Concat(embeddedContext)
	         .ToArray();
	        dump.Resources = templates.GetTemplates()
	                                  .Select(SerializeTemplate)
	                                  .ToArray();

	        // не пишем абсолютных путей в дамп
	        foreach (var script in dump.Scripts)
	        {
		        script.Image.ModuleInfo.Origin = "oscript://" + script.ModuleName();
		        script.Image.ModuleInfo.ModuleName = script.Image.ModuleInfo.Origin;
	        }

	        using (var bw = new BinaryWriter(output))
	        {
		        var serializer = new BinaryFormatter();
		        serializer.Serialize(output, dump);

		        var signature = new byte[]
		        {
			        0x4f,
			        0x53,
			        0x4d,
			        0x44
		        };
		        output.Write(signature, 0, signature.Length);

		        bw.Write(offset);

		        OutputToFile(output);
	        }
        }

	    private ApplicationResource SerializeTemplate(KeyValuePair<string, ITemplate> keyValuePair)
	    {
		    byte[] data;
		    using (var stream = new MemoryStream())
		    {
			    using (var bw = new BinaryWriter(stream))
			    {
				    var buf = keyValuePair.Value.GetBinaryData().Buffer;
				    bw.Write(keyValuePair.Value.Kind.ToString());
				    bw.Write(buf.Length);
				    bw.Write(buf);
				    data = stream.ToArray();
			    }
		    }

		    var appResource = new ApplicationResource
		    {
			    ResourceName = keyValuePair.Key,
			    Data = data
		    };

		    return appResource;
	    }

	    private void CreateExe()
		{
			using (var exeStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("oscript.StandaloneRunner.exe"))
			using (var output = new MemoryStream())
			{
				exeStream?.CopyTo(output);

				CreateDump(output);
			}
		}


		private void OutputToFile(Stream memoryStream)
		{
			using (var fileOutput = new FileStream(_exePath, FileMode.Create))
			{
				memoryStream.Position = 0;
				memoryStream.CopyTo(fileOutput);
			}
		}

		public static AppBehavior Create(CmdLineHelper helper)
		{
			var codepath = helper.Next();
			var output = helper.Next();
			var makeBin = helper.Next();
			if (output == null || codepath == null) 
				return null;
			
			var appMaker = new MakeAppBehavior(codepath, output);
			if (makeBin != null && makeBin == "-bin")
				appMaker.CreateDumpOnly = true;

			return appMaker;

		}
	}
}