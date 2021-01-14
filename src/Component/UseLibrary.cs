/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Collections.ValueList;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.StandardLibrary.Collections.ValueTree;
using OneScript.StandardLibrary.Http;
using OneScript.StandardLibrary.Net;
using OneScript.StandardLibrary.Processes;
using OneScript.StandardLibrary.Text;
using OneScript.StandardLibrary.Xml;
using OneScript.StandardLibrary.Zip;
using OneScript.StandardLibrary.TypeDescriptions;
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

			Use<ArrayImpl>();
			Use<FixedArrayImpl>();
			Use<MapImpl>();
			Use<FixedMapImpl>();
			Use<StructureImpl>();
			Use<FixedStructureImpl>();
			Use<KeyAndValueImpl>();

			Use<ValueTable>();
			Use<ValueTableRow>();
			Use<ValueTableColumn>();
			Use<ValueTableColumnCollection>();

			Use<BinaryDataQualifiers>();
			Use<DateQualifiers>();
			Use<FileContext>();
			Use<GuidWrapper>();
			Use<NumberQualifiers>();
			Use<ProcessContext>();
			Use<ReflectorContext>();
			Use<TypeDescription>();
			Use<TextWriteImpl>();
			Use<TextReadImpl>();
			Use<TextDocumentContext>();
			Use<SystemEnvironmentContext>();
			Use<StringQualifiers>();

			Use<BinaryDataContext>();
			Use<FileStreamContext>();
			Use<GenericStream>();
			Use<MemoryStreamContext>();

			Use<TCPClient>();
			Use<TCPServer>();

			Use<HttpConnectionContext>();
			Use<HttpRequestContext>();
			Use<HttpResponseContext>();
			Use<InternetProxyContext>();

			Use<ValueListImpl>();
			Use<ValueListItem>();

			Use<ValueTree>();
			Use<ValueTreeRow>();
			Use<ValueTreeRowCollection>();
			Use<ValueTreeColumn>();
			Use<ValueTreeColumnCollection>();

			Use<ZipReader>();
			Use<ZipWriter>();
			Use<ZipFileEntryContext>();
			Use<ZipFileEntriesCollection>();

			Use<XmlNamespaceContext>();
			Use<XmlReaderImpl>();
			Use<XmlWriterImpl>();

			Use<StdTextReadStream>();
			Use<StdTextWriteStream>();
		}
	}
}