/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;


namespace Component
{
	public static class UseLibrary
	{

		public static void Use<T>() where T : IValue
		{

		}

		public static void Use()
		{
			/* Суть следующих выражений в том, что они перестанут компилироваться, 
			если какой-либо из классов поменяет область видимости 
			или переедет в иное пространство имён
			(https://github.com/EvilBeaver/OneScript/commit/20e48fa7692b5430b819eb4b6982be1b591e536f)*/

			Use<ScriptEngine.HostedScript.Library.ArrayImpl>();
			Use<ScriptEngine.HostedScript.Library.FixedArrayImpl>();
			Use<ScriptEngine.HostedScript.Library.MapImpl>();
			Use<ScriptEngine.HostedScript.Library.FixedMapImpl>();
			Use<ScriptEngine.HostedScript.Library.StructureImpl>();
			Use<ScriptEngine.HostedScript.Library.FixedStructureImpl>();
			Use<ScriptEngine.HostedScript.Library.KeyAndValueImpl>();

			Use<ScriptEngine.HostedScript.Library.ValueTable.ValueTable>();
			Use<ScriptEngine.HostedScript.Library.ValueTable.ValueTableRow>();
			Use<ScriptEngine.HostedScript.Library.ValueTable.ValueTableColumn>();
			Use<ScriptEngine.HostedScript.Library.ValueTable.ValueTableColumnCollection>();

			Use<ScriptEngine.HostedScript.Library.BinaryDataQualifiers>();
			Use<ScriptEngine.HostedScript.Library.DateQualifiers>();
			Use<ScriptEngine.HostedScript.Library.FileContext>();
			Use<ScriptEngine.HostedScript.Library.GuidWrapper>();
			Use<ScriptEngine.HostedScript.Library.NumberQualifiers>();
			Use<ScriptEngine.HostedScript.Library.ProcessContext>();
			Use<ScriptEngine.HostedScript.Library.ReflectorContext>();
			Use<ScriptEngine.HostedScript.Library.TypeDescription>();
			Use<ScriptEngine.HostedScript.Library.TextWriteImpl>();
			Use<ScriptEngine.HostedScript.Library.TextReadImpl>();
			Use<ScriptEngine.HostedScript.Library.TextDocumentContext>();
			Use<ScriptEngine.HostedScript.Library.SystemEnvironmentContext>();
			Use<ScriptEngine.HostedScript.Library.StringQualifiers>();

			Use<ScriptEngine.HostedScript.Library.Binary.BinaryDataContext>();
			Use<ScriptEngine.HostedScript.Library.Binary.FileStreamContext>();
			Use<ScriptEngine.HostedScript.Library.Binary.GenericStream>();
			Use<ScriptEngine.HostedScript.Library.Binary.MemoryStreamContext>();

			Use<ScriptEngine.HostedScript.Library.Net.TCPClient>();
			Use<ScriptEngine.HostedScript.Library.Net.TCPServer>();

			Use<ScriptEngine.HostedScript.Library.Http.HttpConnectionContext>();
			Use<ScriptEngine.HostedScript.Library.Http.HttpRequestContext>();
			Use<ScriptEngine.HostedScript.Library.Http.HttpResponseContext>();
			Use<ScriptEngine.HostedScript.Library.Http.InternetProxyContext>();

			Use<ScriptEngine.HostedScript.Library.ValueList.ValueListImpl>();
			Use<ScriptEngine.HostedScript.Library.ValueList.ValueListItem>();

			Use<ScriptEngine.HostedScript.Library.ValueTree.ValueTree>();
			Use<ScriptEngine.HostedScript.Library.ValueTree.ValueTreeRow>();
			Use<ScriptEngine.HostedScript.Library.ValueTree.ValueTreeRowCollection>();
			Use<ScriptEngine.HostedScript.Library.ValueTree.ValueTreeColumn>();
			Use<ScriptEngine.HostedScript.Library.ValueTree.ValueTreeColumnCollection>();

			Use<ScriptEngine.HostedScript.Library.Zip.ZipReader>();
			Use<ScriptEngine.HostedScript.Library.Zip.ZipWriter>();
			Use<ScriptEngine.HostedScript.Library.Zip.ZipFileEntryContext>();
			Use<ScriptEngine.HostedScript.Library.Zip.ZipFileEntriesCollection>();

			Use<ScriptEngine.HostedScript.Library.Xml.XmlNamespaceContext>();
			Use<ScriptEngine.HostedScript.Library.Xml.XmlReaderImpl>();
			Use<ScriptEngine.HostedScript.Library.Xml.XmlWriterImpl>();

			Use<ScriptEngine.HostedScript.Library.StdTextReadStream>();
			Use<ScriptEngine.HostedScript.Library.StdTextWriteStream>();

		}
	}
}
