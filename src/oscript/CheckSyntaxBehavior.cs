/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using ScriptEngine.Machine;

namespace oscript
{
	internal class CheckSyntaxBehavior : AppBehavior
	{
		private readonly string _envFile;

		private readonly bool _isCgi;

		private readonly string _path;

		public CheckSyntaxBehavior(string path, string envFile, bool isCgi = false)
		{
			_path = path;
			_envFile = envFile;
			_isCgi = isCgi;
		}

		public override int Execute()
		{
			var builder = ConsoleHostBuilder.Create(_path);
			var hostedScript = ConsoleHostBuilder.Build(builder);
			hostedScript.Initialize();

			if (_isCgi)
			{
				var request = ValueFactory.Create();
				hostedScript.InjectGlobalProperty("ВебЗапрос", "WebRequest", request, true);
			}

			ScriptFileHelper.OnBeforeScriptRead(hostedScript);
			var source = hostedScript.Loader.FromFile(_path);

			hostedScript.SetGlobalEnvironment(new DoNothingHost(), source);

			try
			{
				if (_envFile != null)
				{
					var envCompiler = hostedScript.GetCompilerService();
					var envSrc = hostedScript.Loader.FromFile(_envFile);
					envCompiler.Compile(envSrc);
				}
				var compiler = hostedScript.GetCompilerService();
				compiler.Compile(source);
			}
			catch (ScriptException e)
			{
				Output.WriteLine(e.Message);
				return 1;
			}

			Output.WriteLine("No errors.");

			return 0;
		}

		public static AppBehavior Create(CmdLineHelper helper)
		{
			if (helper.Next() == null) 
				return null;
			
			bool cgiMode = false;
			var arg = helper.Current();
			if (arg.ToLowerInvariant() == "-cgi")
			{
				cgiMode = true;
				arg = helper.Next();
			}
            
			var path = arg;
			var env = helper.Next();
			if (env != null && env.StartsWith("-env="))
			{
				env = env.Substring(5);
			}
            
			return new CheckSyntaxBehavior(path, env, cgiMode);

		}
	}
}