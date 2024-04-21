/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Text;
using oscript.Web.Multipart;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace oscript.Web
{
	[ContextClass("ВебЗапрос", "WebRequest")]
	public class WebRequestContext : AutoContext<WebRequestContext>, IDisposable, IAsyncDisposable
	{
		private PostRequestData _post;

		private FileBackingStream _postBody;

		public WebRequestContext()
		{
			var get = Environment.GetEnvironmentVariable("QUERY_STRING");
			if (get != null) FillGetMap(get);

			ProcessPostData();

			FillEnvironmentVars();
		}

		/// <summary>
		///     Параметры запроса
		/// </summary>
		[ContextProperty("Параметры", "Params")]
		public FixedMapImpl Params => _post.Params;

		/// <summary>
		///     Загруженные файлы
		/// </summary>
		[ContextProperty("Файлы", "Files")]
		public FixedMapImpl Files => _post.Files;

		/// <summary>
		///     Переменные среды
		/// </summary>
		[ContextProperty("ENV")]
		public FixedMapImpl ENV { get; private set; }

		private void ProcessPostData()
		{
			var contentLen = Environment.GetEnvironmentVariable("CONTENT_LENGTH");
			if (contentLen == null)
				return;

			var len = int.Parse(contentLen);
			if (len == 0)
				return;
			
			var type = Environment.GetEnvironmentVariable("CONTENT_TYPE");
			
			using var stdin = Console.OpenStandardInput();
			var dest = new FileBackingStream(FileBackingConstants.DEFAULT_MEMORY_LIMIT, len);
			stdin.CopyTo(dest);
			dest.Position = 0;
			
			if (type != null && type.StartsWith("multipart/"))
			{
				var boundary = type.Substring(type.IndexOf('=') + 1);
				_post = new PostRequestData(dest, boundary);
			}
			else
			{
				using var reader = new StreamReader(dest, Encoding.UTF8, leaveOpen: true);
				_post = new PostRequestData(reader.ReadToEnd());
			}

			dest.Position = 0;
			_postBody = dest;
		}

		private void FillEnvironmentVars()
		{
			var vars = new MapImpl();
			foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
				vars.Insert(
					ValueFactory.Create((string) item.Key),
					ValueFactory.Create((string) item.Value));

			ENV = new FixedMapImpl(vars);
		}

		private void FillGetMap(string get)
		{
			_post = new PostRequestData(get);
		}

		[ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
		public BinaryDataContext GetBodyAsBinaryData()
		{
			_postBody.Position = 0;
			return new BinaryDataContext(_postBody);
		}

		[ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
		public string GetBodyAsString(IValue encoding = null)
		{
			var enc = encoding == null || ValueFactory.Create().Equals(encoding)
				? new UTF8Encoding(false)
				: TextEncodingEnum.GetEncoding(encoding);

			_postBody.Position = 0;
			using var streamReader = new StreamReader(_postBody, enc);
			return streamReader.ReadToEnd();
		}

		public void Dispose()
		{
			_postBody?.Dispose();
		}

		public async ValueTask DisposeAsync()
		{
			if (_postBody != null) await _postBody.DisposeAsync();
		}
	}
}