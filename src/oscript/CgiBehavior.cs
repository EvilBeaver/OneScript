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
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.Types;
using oscript.Web;

using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace oscript
{
	internal class CgiBehavior : AppBehavior, IHostApplication, IRuntimeContextInstance, IAttachableContext
	{
		private static readonly ContextMethodsMapper<CgiBehavior> _methods = new ContextMethodsMapper<CgiBehavior>();

		private readonly HashSet<string> _headersWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private bool _isContentEchoed;

		public CgiBehavior()
		{
			Encoding = new UTF8Encoding();
		}

		public override int Execute()
		{
			var scriptFile = Environment.GetEnvironmentVariable("SCRIPT_FILENAME") ?? Environment.GetEnvironmentVariable("PATH_TRANSLATED");

			if (scriptFile == null)
			{
				Header("Content-type", "text/plain");
				Echo("No CGI Variables found");
				return 1;
			}

			if (!File.Exists(scriptFile))
			{
				Header("Content-type", "text/plain");
				Echo($"Script file not found: {scriptFile}");
				return 1;
			}

			return RunCGIMode(scriptFile);
		}

		private int RunCGIMode(string scriptFile)
		{
			var builder = ConsoleHostBuilder
				.Create(scriptFile);

			builder.SetupEnvironment(e =>
				{
					e.AddAssembly(GetType().Assembly);
				});

			var engine = ConsoleHostBuilder.Build(builder);
			
			var request = new WebRequestContext();
			engine.InjectGlobalProperty("ВебЗапрос", "WebRequest", request, true);
			engine.InjectObject(this, false);

			ScriptFileHelper.OnBeforeScriptRead(engine);
			var source = engine.Loader.FromFile(scriptFile);

			Process process;

			try
			{
				process = engine.CreateProcess(this, source);
			}
			catch (Exception e)
			{
				ShowExceptionInfo(e);
				return 1;
			}

			var exitCode = process.Start();

			if (!_isContentEchoed)
				Echo("");

			return exitCode;
		}

		public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodSignature[] methods)
		{
			variables = new IVariable[0];
			methods = this.GetMethods().Select(x=>x.MakeSignature()).ToArray();
		}

		#region CGIHost

		[ContextMethod("ВывестиЗаголовок", "Header")]
		public void Header(string header, string value)
		{
			if (_isContentEchoed)
				throw new InvalidOperationException("Headers can not be written after the main content");

			Output(header + ": " + value + "\r\n");
			_headersWritten.Add(header);
		}

		[ContextMethod("ОтправитьФайл", "SendFile")]
		public void SendFile(string filePath, string downloadFileName = null)
		{
			if (_isContentEchoed)
				throw new InvalidOperationException("Content already sent!");

			if (!IsHeaderWritten("Content-type"))
				Header("Content-type", "application/octet-stream");
			if (string.IsNullOrEmpty(downloadFileName))
			{
				var finfo = new FileInfo(filePath);
				downloadFileName = finfo.Name;
			}
			using (var fs = new FileStream(filePath, FileMode.Open))
			{
				Header("Content-disposition", $"inline; filename=\"{downloadFileName}\"");
				Header("Content-length", fs.Length.ToString());
				oscript.Output.WriteLine();

				using (var stdout = Console.OpenStandardOutput())
				{
					fs.CopyTo(stdout);
				}
			}
		}

		public Encoding Encoding { get; set; }

		private bool IsHeaderWritten(string header)
		{
			return _headersWritten.Contains(header);
		}

		public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
		{
			if (!_isContentEchoed)
			{
				if (!IsHeaderWritten("Content-type"))
					Header("Content-type", "text/html");
				if (!IsHeaderWritten("Content-encoding"))
					Header("Content-encoding", Encoding.BodyName);
				oscript.Output.WriteLine();

				_isContentEchoed = true;
			}

			if (str != "")
			{
				Output(str);
				oscript.Output.WriteLine();
			}
		}

		private void Output(string str)
		{
			using (var stream = Console.OpenStandardOutput())
			{
				var bytes = Encoding.GetBytes(str);
				stream.Write(bytes, 0, bytes.Length);
			}
		}

		public void ShowExceptionInfo(Exception exc)
		{
			Echo(exc.ToString());
		}

		public bool InputString(out string result, string prompt, int maxLen, bool multiline)
		{
			result = null;
			return false;
		}

		public string[] GetCommandLineArguments()
		{
			return new string[0];
		}

		#endregion

		#region IRuntimeContextInstance Members

		public bool IsIndexed => false;

		public bool DynamicMethodSignatures => false;

		public IValue GetIndexedValue(IValue index)
		{
			throw new NotImplementedException();
		}

		public void SetIndexedValue(IValue index, IValue val)
		{
			throw new NotImplementedException();
		}

		public int FindProperty(string name)
		{
			throw PropertyAccessException.PropNotFoundException(name);
		}

		public bool IsPropReadable(int propNum)
		{
			return false;
		}

		public bool IsPropWritable(int propNum)
		{
			return false;
		}

		public IValue GetPropValue(int propNum)
		{
			throw new ArgumentException();
		}

		public void SetPropValue(int propNum, IValue newVal)
		{
			throw new InvalidOperationException("global props are not writable");
		}

		public int FindMethod(string name)
		{
			return _methods.FindMethod(name);
		}
		
		public BslMethodInfo GetMethodInfo(int methodNumber)
		{
			return _methods.GetRuntimeMethod(methodNumber);
		}

		public BslPropertyInfo GetPropertyInfo(int propertyNumber)
		{
			throw new NotImplementedException();
		}
		
		public void CallAsProcedure(int methodNumber, IValue[] arguments)
		{
			_methods.GetCallableDelegate(methodNumber)(this, arguments);
		}

		public void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
		{
			retValue = _methods.GetCallableDelegate(methodNumber)(this, arguments);
		}

        public int GetPropCount()
        {
            return 0;
        }

        public string GetPropName(int propNum)
        {
            throw new NotImplementedException();
        }

        public int GetMethodsCount()
        {
            return _methods.Count;
        }

        #endregion
    }
}