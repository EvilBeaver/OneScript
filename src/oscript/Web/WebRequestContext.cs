/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections;
using System.Text;

using oscript.Web.Multipart;

using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace oscript.Web
{
	[ContextClass("ВебЗапрос", "WebRequest")]
	public class WebRequestContext : AutoContext<WebRequestContext>
	{
		private PostRequestData _post;

		private byte[] _postRaw;

		public WebRequestContext()
		{
			var get = Environment.GetEnvironmentVariable("QUERY_STRING");
			if (get != null) FillGetMap(get);

			ProcessPostData();

			FillEnvironmentVars();
		}

		[Obsolete]
		[ContextProperty("GET")]
		public IValue GET => _post.Params;

		[Obsolete]
		[ContextProperty("POST")]
		public IValue POST => _post;

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

			_postRaw = new byte[len];
			using (var stdin = Console.OpenStandardInput())
			{
				stdin.Read(_postRaw, 0, len);
			}

			var type = Environment.GetEnvironmentVariable("CONTENT_TYPE");
			if (type != null && type.StartsWith("multipart/"))
			{
				var boundary = type.Substring(type.IndexOf('=') + 1);
				_post = new PostRequestData(_postRaw, boundary);
			}
			else
			{
				_post = new PostRequestData(Encoding.UTF8.GetString(_postRaw));
			}
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
			return new BinaryDataContext(_postRaw);
		}

		[ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
		public string GetBodyAsString(IValue encoding = null)
		{
			var enc = encoding == null || ValueFactory.Create().Equals(encoding)
				? new UTF8Encoding(false)
				: TextEncodingEnum.GetEncoding(encoding);

			return enc.GetString(_postRaw);
		}
	}
}