/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.Binary;

/// <summary>
/// 
/// </summary>

namespace ScriptEngine.HostedScript.Library.HTTPService
{
    /* HTTPСервисЗапрос
    * Свойства:

      +HTTPМетод (HTTPMethod)
      +БазовыйURL (BaseURL)
      +Заголовки (Headers)
      +ОтносительныйURL (RelativeURL)
      +ПараметрыURL (URLParameters)
      +ПараметрыЗапроса (QueryOptions)

      Методы:

      +ПолучитьТелоКакДвоичныеДанные (GetBodyAsBinaryData)
      ПолучитьТелоКакПоток (GetBodyAsStream)
      +ПолучитьТелоКакСтроку (GetBodyAsString)

    */
    [ContextClass("HTTPСервисЗапрос", "HTTPServiceRequest")]
    public class HTTPServiceRequestImpl : AutoContext<HTTPServiceRequestImpl>
    {
        System.Web.HttpContext _context;

        FixedMapImpl _headers;
        FixedMapImpl _urlParams;
        FixedMapImpl _queryOptions;

        #region Свойства 1C
        [ContextProperty("Контекст", "Context")]
        public HTTPServiceContextImpl Context
        {
            get
            {
                return new HTTPServiceContextImpl(_context);
            }
        }


        [ContextProperty("HTTPМетод", "HTTPMethod")]
        public string HTTPMethod
        {
            get
            {
                return _context.Request.HttpMethod.ToUpper();
            }
        }

        [ContextProperty("БазовыйURL", "BaseURL")]
        public string BaseURL
        {
            get
            {
                return _context.Request.Url.Host;
            }
        }

        [ContextProperty("Заголовки", "Headers")]
        public FixedMapImpl Headers
        {
            get
            {
                return _headers;
            }
        }

        [ContextProperty("ОтносительныйURL", "RelativeURL")]
        public string RelativeURL
        {
            get
            {
                return _context.Request.FilePath;
            }
        }

        [ContextProperty("ПараметрыURL", "URLParameters")]
        public FixedMapImpl URLParameters
        {
            get
            {
                return _urlParams;
            }
        }

        [ContextProperty("ПараметрыЗапроса", "QueryOptions")]
        public FixedMapImpl QueryOptions
        {
            get
            {
                return _queryOptions;
            }
        }
        #endregion

        #region Методы1С

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public BinaryDataContext GetBodyAsBinaryData()
        {
            System.IO.Stream str = _context.Request.InputStream;
            int bytes_count = Convert.ToInt32(str.Length);
            byte[] buffer = new byte[bytes_count];
            str.Seek(0, System.IO.SeekOrigin.Begin);
            str.Read(buffer, 0, bytes_count);

            return new BinaryDataContext(buffer);
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public string GetBodyAsString(IValue encoding = null)
        {
            // Формируем кодировку как в 1С. Если не указана, смотрим Content-Type. Если там не указана - используем UTF8
            System.Text.Encoding enc = System.Text.Encoding.UTF8;

            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);
            else
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("charset=([^\\\"']+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                string charsetString = regex.Match(_context.Request.ContentType).Value;

                if (charsetString != "")
                {
                    // Что-то нашли 
                    try
                    {
                        //charsetString.Substring(8) делает "charset=Кодировка" -> "Кодировка" 
                        enc = TextEncodingEnum.GetEncodingByName(charsetString.Substring(8));
                    }
                    catch
                    {
                        // что то не так, осталась UTF8  
                    }
                }
            }

            System.IO.Stream str = _context.Request.InputStream;
            int bytes_count = Convert.ToInt32(str.Length);
            byte[] buffer = new byte[bytes_count];

            str.Seek(0, System.IO.SeekOrigin.Begin);
            str.Read(buffer, 0, bytes_count);

            return enc.GetString(buffer);
        }

        //ПолучитьТелоКакПоток(GetBodyAsStream)
        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_context.Request.InputStream);
        }

        #endregion

        public HTTPServiceRequestImpl(System.Web.HttpContext ctx)
        {
            _context = ctx;
            // Инициализируем объект для 1С
            // Заголовки
            MapImpl headers = new MapImpl();

            for (int i = 0; i < _context.Request.Headers.Count; i++)
                headers.Insert(ValueFactory.Create(_context.Request.Headers.GetKey(i))
                              , ValueFactory.Create(_context.Request.Headers.Get(i))
                              );

            this._headers = new FixedMapImpl(headers);

            // ПараметрыURL будут пустыми
            _urlParams = new FixedMapImpl(new MapImpl());

            // Параметры запроса
            MapImpl queryOptions = new MapImpl();

            for (int i = 0; i < _context.Request.Params.Count; i++)
                queryOptions.Insert(ValueFactory.Create(_context.Request.Params.GetKey(i))
                                   , ValueFactory.Create(_context.Request.Params.Get(i))
                                   );

            _queryOptions = new FixedMapImpl(queryOptions);
        }
    }
}