/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using ScriptEngine.Machine;

namespace oscript
{
	public class CodeStatWriter
	{
		private readonly string _outputFileName;

		private readonly CodeStatWriterType _type;

		public CodeStatWriter(string fileName, CodeStatWriterType type)
		{
			_outputFileName = fileName;
			_type = type;
		}

		public void Write(CodeStatDataCollection codeStatDataCollection)
		{
			if (_type == CodeStatWriterType.JSON)
				WriteToJson(codeStatDataCollection);
			else
				throw new ArgumentException("Unsupported type");
		}

		private void WriteToJson(CodeStatDataCollection codeStatDataCollection)
		{
			using (var w = new StreamWriter(_outputFileName))
			{
				var jwriter = new JsonTextWriter(w)
				{
					Formatting = Formatting.Indented
				};

				jwriter.WriteStartObject();
				foreach (var source in codeStatDataCollection.GroupBy(arg => arg.Entry.ScriptFileName))
				{
					jwriter.WritePropertyName(source.Key, true);
					jwriter.WriteStartObject();

					jwriter.WritePropertyName("#path");
					jwriter.WriteValue(source.Key);
					foreach (var method in source.GroupBy(arg => arg.Entry.SubName))
					{
						jwriter.WritePropertyName(method.Key, true);
						jwriter.WriteStartObject();

						foreach (var entry in method.OrderBy(kv => kv.Entry.LineNumber))
						{
							jwriter.WritePropertyName(entry.Entry.LineNumber.ToString());
							jwriter.WriteStartObject();

							jwriter.WritePropertyName("count");
							jwriter.WriteValue(entry.ExecutionCount);

							jwriter.WritePropertyName("time");
							jwriter.WriteValue(entry.TimeElapsed);

							jwriter.WriteEndObject();
						}

						jwriter.WriteEndObject();
					}

					jwriter.WriteEndObject();
				}

				jwriter.WriteEndObject();
				jwriter.Flush();
			}
		}
	}

	//TODO: Добавить другие форматы записи
	public enum CodeStatWriterType
	{
		JSON
	}
}