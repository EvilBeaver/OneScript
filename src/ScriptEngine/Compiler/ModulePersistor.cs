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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using ScriptEngine.Environment;

namespace ScriptEngine.Compiler
{
	public class ModulePersistor
	{
		private struct ModuleHeader
		{
			public int NameLen;

			public long BodyLen;

			public bool IsClass;
		}

		private readonly IFormatter _formatter;

		public ModulePersistor(IFormatter format)
		{
			_formatter = format;
		}

		public ModulePersistor()
		{
			_formatter = new BinaryFormatter();
		}

		public static void WriteStruct<T>(Stream s, T value)
		{
			var rawsize = Marshal.SizeOf(typeof(T));
			var rawdata = new byte[rawsize];
			var handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
			Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
			handle.Free();
			s.Write(rawdata, 0, rawsize);
		}

		public static T ReadStruct<T>(Stream s)
		{
			var buffer = new byte[Marshal.SizeOf(typeof(T))];
			s.Read(buffer, 0, Marshal.SizeOf(typeof(T)));

			var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			var temp = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
			handle.Free();
			return temp;
		}

		public void Save(UserAddedScript script, Stream output)
		{
			var nameBytes = Encoding.UTF8.GetBytes(script.Symbol);
			var bodyStream = new MemoryStream(8 * 1024);
			_formatter.Serialize(bodyStream, script.Image);

			var header = new ModuleHeader
			{
				NameLen = nameBytes.Length,
				BodyLen = bodyStream.Length,
				IsClass = script.Type == UserAddedScriptType.Class
			};

			WriteStruct(output, header);

			output.Write(nameBytes, 0, nameBytes.Length);
			bodyStream.Position = 0;
			bodyStream.WriteTo(output);
			bodyStream.Dispose();
		}
        
		public UserAddedScript Read(Stream input)
		{
			var header = ReadStruct<ModuleHeader>(input);

			var userScript = new UserAddedScript();

			var nameBytes = new byte[header.NameLen];
			input.Read(nameBytes, 0, nameBytes.Length);

			userScript.Symbol = Encoding.UTF8.GetString(nameBytes);

			var moduleImage = (ModuleImage) _formatter.Deserialize(input);

			var path = Assembly.GetEntryAssembly().Location;
			moduleImage.ModuleInfo = new ModuleInformation
			{
				CodeIndexer = new CompiledCodeIndexer(),
				ModuleName = string.Format("{0}:{1}", Path.GetFileName(path), userScript.Symbol),
				Origin = path
			};

		    userScript.Image = moduleImage;
			userScript.Type = header.IsClass ? UserAddedScriptType.Class : UserAddedScriptType.Module;

			return userScript;
		}
	}
}