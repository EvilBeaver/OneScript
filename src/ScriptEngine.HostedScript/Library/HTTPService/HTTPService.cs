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

            System.IO.Stream str = _context.Request.InputStream;
            int bytes_count = Convert.ToInt32(str.Length);
            byte[] buffer = new byte[bytes_count];
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
                 headers.Insert( ValueFactory.Create(_context.Request.Headers.GetKey(i))
                               , ValueFactory.Create(_context.Request.Headers.Get(i))
                               );

            this._headers = new FixedMapImpl(headers);

            // ПараметрыURL будут пустыми
            _urlParams = new FixedMapImpl(new MapImpl());

            // Параметры запроса
            MapImpl queryOptions = new MapImpl();

            for (int i = 0; i < _context.Request.Params.Count; i++)
                queryOptions.Insert( ValueFactory.Create(_context.Request.Params.GetKey(i))
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

        System.IO.Stream _stream = null;

        public byte[] Body
        {
            get
            {
                return GetBytesFromStream();
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                return _stream;
            }
        }

        public HTTPServiceResponseImpl()
        {
        }

        byte [] GetBytesFromStream()
        {
            byte[] buffer = new byte[_stream.Length];
            long currentOffset = _stream.Position;

            _stream.Seek(0, System.IO.SeekOrigin.Begin);
            _stream.Read(buffer, 0, (int)_stream.Length);
            _stream.Seek(currentOffset, System.IO.SeekOrigin.Begin);

            return buffer;
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
            if ((_stream as System.IO.FileStream) == null)
                return null;
            else
                return ValueFactory.Create(((System.IO.FileStream)_stream).Name);
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public BinaryDataContext ПолучитьТелоКакДвоичныеДанные()
        {
            if ((_stream as System.IO.MemoryStream) == null)
                return null;
            else
            {
                return new BinaryDataContext(GetBytesFromStream());
            }
        }

        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public GenericStream GetBodyAsStream()
        {
            if (_stream == null)
                _stream = new System.IO.MemoryStream();
                
            return new GenericStream(_stream);
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            if ((_stream as System.IO.MemoryStream) == null )
                return ValueFactory.Create();
            else
            {
                byte[] buffer = GetBytesFromStream();
                // Выяснено экспериментальным путем, используется UTF8 (8.3.10.2650)
                return ValueFactory.Create(System.Text.Encoding.UTF8.GetString(buffer));
            }
        }

        [ContextMethod("УстановитьИмяФайлаТела", "SetBodyFileName")]
        public void SetBodyFileName(IValue fileName)
        {
            _stream = new System.IO.FileStream(fileName.AsString(), System.IO.FileMode.Open, System.IO.FileAccess.Read);
        }

        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinaryData")]
        public void SetBodyFromBinaryData(BinaryDataContext binaryData)
        {
            _stream = new System.IO.MemoryStream();
            _stream.Write(binaryData.Buffer, 0, binaryData.Buffer.Length);
            _stream.Seek(0, System.IO.SeekOrigin.Begin);
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
            else
            {
                string headerString = _headers.Retrieve(ValueFactory.Create("Content-Type")).AsString();
                  
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("charset=([^\\\"']+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                string charsetString = regex.Match(headerString).Value;

                if (charsetString != "")
                {
                    // Что-то нашли
                    try
                    {
                        //"charset=Кодировка" -> "Кодировка"
                        enc = TextEncodingEnum.GetEncodingByName(charsetString.Substring(8));
                    }
                    catch
                    {
                        // что то не так, осталась UTF8 
                    }
                }
            }

            _stream = new System.IO.MemoryStream();
            byte[] buffer = enc.GetBytes(str);
            _stream.Write(buffer, 0, buffer.Length);
            _stream.Seek(0, System.IO.SeekOrigin.Begin);
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

    [ContextClass("HTTPСервисКонтекст", "HTTPServiceContext")]
    public class HTTPServiceContext : AutoContext<HTTPServiceContext>
    {
        System.Web.HttpContext context;

        HTTPServiceRequestImpl request;
        HTTPServiceResponseImpl response;

        public HTTPServiceContext(System.Web.HttpContext ctx)
        {
            context = ctx;
            request = new HTTPServiceRequestImpl(context);
            response = new HTTPServiceResponseImpl();
        }

        public HTTPServiceContext()
        {
            context = null;
            request = null;
            response = null;
        }

        public void SetHTTPContext(System.Web.HttpContext ctx)
        {
            context = ctx;
            request = new HTTPServiceRequestImpl(context);
            response = new HTTPServiceResponseImpl();
        }

        #region Свойства для 1C

        [ContextProperty("Запрос", "Request")]
        public HTTPServiceRequestImpl Request
        {
            get
            {
                return request;
            }
        }

        [ContextProperty("Ответ", "Response")]
        public HTTPServiceResponseImpl Response
        {
            get
            {
                return response;
            }
            set
            {
                response = value;
            }
        }
        #endregion

        #region Функции 1С
        #endregion
    }
}
