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

    /*
        8.3.10.2650

        HTTPСервисОтвет (HTTPServiceResponseImpl)
        
        Свойства:

        +Заголовки (Headers) - Соответствие
        +КодСостояния (StatusCode) - Целое
        +Причина (Reason) - Строка

        Методы:

        +ПолучитьИмяФайлаТела (GetBodyFileName)
        +ПолучитьТелоКакДвоичныеДанные (GetBodyAsBinaryData)
        +ПолучитьТелоКакСтроку (GetBodyAsString)
        +ПолучитьТелоКакПоток (GetBodyAsStream)
        +УстановитьИмяФайлаТела (SetBodyFileName)
        +УстановитьТелоИзДвоичныхДанных (SetBodyFromBinaryData)
        +УстановитьТелоИзСтроки (SetBodyFromString)

        Конструкторы:

        +По коду состояния, причине и заголовкам

        ОТЛИЧИЯ:
        При возврате потоков в несколько переменных, в 1С не обновляется текущее положение потока,
        в настоящей реализации обновляется
    */

    [ContextClass("HTTPСервисОтвет", "HTTPServiceResponse")]
    public class HTTPServiceResponseImpl : AutoContext<HTTPServiceResponseImpl>
    {
        ScriptEngine.HostedScript.Library.MapImpl _headers = new HostedScript.Library.MapImpl();
        string _reason = "";
        int _statusCode = 200;

        System.IO.Stream _bodyStream = new System.IO.MemoryStream();

        public System.IO.Stream BodyStream
        {
            get
            {
                return _bodyStream;
            }
        }

        public HTTPServiceResponseImpl()
        {
        }

        #region Свойства 1C

        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers
        {
            get
            {
                return _headers;
            }
            set
            {
                _headers = value;
            }
        }

        [ContextProperty("Причина", "Reason")]
        public string Reason
        {
            get
            {
                return _reason;
            }
            set
            {
                _reason = value;
            }
        }

        [ContextProperty("КодСостояния", "StatusCode")]
        public int StatusCode
        {
            get
            {
                return _statusCode;
            }
            set
            {
                _statusCode = value;
            }
        }

        #endregion

        #region Функции 1С

        [ContextMethod("ПолучитьИмяФайлаТела", "GetBodyFileName")]
        public IValue GetBodyFileName()
        {
            if ((_bodyStream as System.IO.FileStream) == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(((System.IO.FileStream)_bodyStream).Name);
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public BinaryDataContext ПолучитьТелоКакДвоичныеДанные()
        {
            if ((_bodyStream as System.IO.MemoryStream) == null)
                return null;
            else
                return new BinaryDataContext( ((System.IO.MemoryStream)_bodyStream).GetBuffer() );
        }

        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_bodyStream);
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            if ((_bodyStream as System.IO.MemoryStream) == null)
                return ValueFactory.Create();
            else
            {
                // Выяснено экспериментальным путем, используется UTF8 (8.3.10.2650)
                return ValueFactory.Create(System.Text.Encoding.UTF8.GetString(((System.IO.MemoryStream)_bodyStream).GetBuffer()));
            }
        }

        [ContextMethod("УстановитьИмяФайлаТела", "SetBodyFileName")]
        public void SetBodyFileName(IValue fileName)
        {
            _bodyStream = new System.IO.FileStream(fileName.AsString(), System.IO.FileMode.Open, System.IO.FileAccess.Read);
        }

        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinaryData")]
        public void SetBodyFromBinaryData(BinaryDataContext binaryData)
        {
            _bodyStream = new System.IO.MemoryStream();
            _bodyStream.Write(binaryData.Buffer, 0, binaryData.Buffer.Length);
            _bodyStream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string str, IValue encoding = null, IValue useBOM = null)
        {
            // Получаем кодировку
            // useBOM должен иметь тип ИспользованиеByteOrderMark он нереализован. Его не используем
            // Из синтаксис-помощника в режиме совместимости Использовать
            // Из синтаксис помощника если кодировка не задана используем UTF8

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);
 
            _bodyStream = new System.IO.MemoryStream();
            byte[] buffer = enc.GetBytes(str);
            _bodyStream.Write(buffer, 0, buffer.Length);
            _bodyStream.Seek(0, System.IO.SeekOrigin.Begin);
        }

        [ScriptConstructor(Name = "По коду состояния, причине и заголовкам")]
        public static IRuntimeContextInstance Constructor(IValue statusCode, IValue reason = null, MapImpl headers = null)
        {
            var response = new HTTPServiceResponseImpl();

            response._statusCode = System.Convert.ToInt16(statusCode.AsNumber());

            if (reason != null)
                response._reason = reason.AsString();

            if (headers != null)
                response._headers = headers;

            return response;
        }

        #endregion
    }
}