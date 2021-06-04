/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;
using ScriptEngine.Compiler;

namespace oscript
{
	internal class ShowCompiledBehavior : AppBehavior
	{
		private readonly string _path;

		public ShowCompiledBehavior(string path)
		{
			_path = path;
		}

		public override int Execute()
		{
			var builder = ConsoleHostBuilder.Create(_path);
			var hostedScript = ConsoleHostBuilder.Build(builder);
			hostedScript.Initialize();
			ScriptFileHelper.OnBeforeScriptRead(hostedScript);
			
			var source = hostedScript.Loader.FromFile(_path);
			var compiler = hostedScript.GetCompilerService();
			hostedScript.SetGlobalEnvironment(new DoNothingHost(), source);
			var writer = new ModuleWriter(compiler);
			try
			{
				writer.Write(Console.Out, source);
			}
			catch (ScriptException e)
			{
				Output.WriteLine(e.Message);
				return 1;
			}

			return 0;
		}

		public static AppBehavior Create(CmdLineHelper helper)
		{
			var path = helper.Next();
			return path != null ? new ShowCompiledBehavior(path) : null;
		}
	}
}