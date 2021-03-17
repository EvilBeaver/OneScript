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
using System.Text;

namespace StandaloneRunner
{
	internal static class Program
	{
		private static Dictionary<string, Assembly> _loadedAssemblies = new Dictionary<string, Assembly>();

		private static int Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			return Run(args);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private static int Run(string[] args)
		{
			if (args.Length > 1 && args[0] == "-loadDump")
			{
				var path = args[1];
				return RunExternalDump(path, args.Skip(2).ToArray());
			}

			return RunEmbeddedDump(args);
		}

		private static int RunEmbeddedDump(string[] args)
		{
			var fileName = Assembly.GetExecutingAssembly().Location;
			using (var sourceStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				return LoadAndRun(sourceStream, args);
			}
		}

		private static int RunExternalDump(string path, string[] args)
		{
			using (var sourceStream = new FileStream(path, FileMode.Open))
			{
				return LoadAndRun(sourceStream, args);
			}
		}

		private static int LoadAndRun(FileStream sourceStream, string[] args)
		{
			var codeStream = LocateCode(sourceStream);
			var process = new StandaloneProcess
			{
				CommandLineArguments = args
			};

			return process.LoadAndRun(codeStream);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assemblyShortName = new AssemblyName(args.Name).Name;
			var resourceName = "StandaloneRunner." + assemblyShortName + ".dll";
			Assembly assembly;

			if (_loadedAssemblies.TryGetValue(assemblyShortName, out assembly))
			{
				return assembly;
			}

			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
			{
				var asmData = new byte[stream.Length];
				stream.Read(asmData, 0, asmData.Length);
				assembly = Assembly.Load(asmData);

				_loadedAssemblies.Add(assemblyShortName, assembly);
				return assembly;
			}
		}

		private static Stream LocateCode(Stream sourceStream)
		{
			const int SIGN_SIZE = 8;
			sourceStream.Position = sourceStream.Length - SIGN_SIZE;
			var signature = new byte[SIGN_SIZE];
			sourceStream.Read(signature, 0, SIGN_SIZE);

			if (signature[0] == 0x4f && signature[1] == 0x53 && signature[2] == 0x4d && signature[3] == 0x44)
			{
				var codeOffset = BitConverter.ToInt32(signature, 4);
				var codeLen = sourceStream.Length - codeOffset - SIGN_SIZE;

				sourceStream.Seek(codeOffset, SeekOrigin.Begin);
				var code = new byte[codeLen];
				sourceStream.Read(code, 0, (int)codeLen);
				var ms = new MemoryStream(code);

				return ms;
			}

			throw new InvalidOperationException("No module found");
		}
	}
}