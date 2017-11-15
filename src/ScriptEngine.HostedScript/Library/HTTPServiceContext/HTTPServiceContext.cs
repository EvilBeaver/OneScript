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
    #region Заимствованные шаблоны, чтобы не лазить по исходникам
    /*
    public interface IValue : IComparable<IValue>, IEquatable<IValue>
    {
        DataType DataType { get; }
        TypeDescriptor SystemType { get; }

        decimal AsNumber();
        DateTime AsDate();
        bool AsBoolean();
        string AsString();
        IRuntimeContextInstance AsObject();
        IValue GetRawValue();

    }
    */
    #endregion

    // Служебный класс, нужен для заполнения объекта фиксированное соответствие строками
    // По другому как заполнить не разобрался
    class StringValue : IValue
    {
        string str = "";

        public StringValue(string str)
        {
            this.str = str;
        }
        public StringValue()
        {
            str = "";
        }
        public DataType DataType
        {
            get { return Machine.DataType.String; }
        }

        public TypeDescriptor SystemType
        {
            get { return TypeManager.GetTypeByFrameworkType(typeof(StringValue)); }
        }

        public decimal AsNumber()
        {
            return System.Convert.ToDecimal(str);
        }

        public DateTime AsDate()
        {
            throw RuntimeException.ConvertToDateException();
        }

        public bool AsBoolean()
        {
            throw RuntimeException.ConvertToBooleanException();
        }

        public string AsString()
        {
            return str;
        }

        public IRuntimeContextInstance AsObject()
        {
            throw RuntimeException.ValueIsNotObjectException();
        }

        public IValue GetRawValue()
        {
            return this;
        }

        public int CompareTo(IValue other)
        {
            if (other.DataType == DataType.String)
            {
                return System.String.Compare(str, other.AsString());
            }

            throw RuntimeException.ComparisonNotSupportedException();
        }

        public bool Equals(IValue other)
        {
            return other.GetRawValue() == this;
        }

    }

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
    public class HTTPServiceRequest : AutoContext<HTTPServiceRequest>
    {
        System.Web.HttpContext context;

        FixedMapImpl headers;
        FixedMapImpl url_params;
        FixedMapImpl query_options;

        #region Свойства 1C

        [ContextProperty("HTTPМетод", "HTTPMethod")]
        public string HTTPMethod
        {
            get
            {
                return context.Request.HttpMethod.ToUpper();
            }
        }

        [ContextProperty("БазовыйURL", "BaseURL")]
        public string BaseURL
        {
            get
            {
                return context.Request.Url.Host;
            }
        }

        [ContextProperty("Заголовки", "Headers")]
        public FixedMapImpl Headers
        {
            get
            {
                return headers;
            }
        }

        [ContextProperty("ОтносительныйURL", "RelativeURL")]
        public string RelativeURL
        {
            get
            {
                return context.Request.FilePath;
            }
        }

        [ContextProperty("ПараметрыURL", "URLParameters")]
        public FixedMapImpl URLParameters
        {
            get
            {
                return url_params;
            }
        }

        [ContextProperty("ПараметрыЗапроса", "QueryOptions")]
        public FixedMapImpl QueryOptions
        {
            get
            {
                return query_options;
            }
        }
        #endregion
        #region Методы1С

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public string GetBodyAsString()
        {
            System.IO.Stream str = context.Request.InputStream;
            int bytes_count = Convert.ToInt32(str.Length);
            byte[] buffer = new byte[bytes_count];
            str.Read(buffer, 0, bytes_count);
            return context.Request.ContentEncoding.GetString(buffer);
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public BinaryDataContext GetBodyAsBinaryData()
        {
            System.IO.Stream str = context.Request.InputStream;
            int bytes_count = Convert.ToInt32(str.Length);
            byte[] buffer = new byte[bytes_count];
            str.Read(buffer, 0, bytes_count);
            return new BinaryDataContext(buffer);
        }

        #endregion

        public HTTPServiceRequest(System.Web.HttpContext ctx)
        {
            context = ctx;
            // Инициализируем объект для 1С
            // Заголовки
            MapImpl headers = new MapImpl();

            for (int i = 0; i < context.Request.Headers.Count; i++)
            {
                headers.Insert(new StringValue(context.Request.Headers.GetKey(i)), new StringValue(context.Request.Headers.Get(i)));
            }

            this.headers = new FixedMapImpl(headers);

            // ПараметрыURL будут пустыми
            url_params = new FixedMapImpl(new MapImpl());

            // Параметры запроса
            MapImpl queryoptions = new MapImpl();

            for (int i = 0; i < context.Request.Params.Count; i++)
            {
                queryoptions.Insert(new StringValue(context.Request.Params.GetKey(i)), new StringValue(context.Request.Params.Get(i)));
            }

            query_options = new FixedMapImpl(queryoptions);
        }
    }

    /*
        HTTPСервисОтвет (HTTPServiceResponse)
        
        Свойства:

        +Заголовки (Headers) - Соответствие
        +КодСостояния (StatusCode) - Целое
        +Причина (Reason) - Строка

        Методы:

        ПолучитьИмяФайлаТела (GetBodyFileName)
        ПолучитьТелоКакДвоичныеДанные (GetBodyAsBinaryData) - Двоичные данные
        +-ПолучитьТелоКакСтроку (GetBodyAsString) - Строка
        УстановитьИмяФайлаТела (SetBodyFileName)
        УстановитьТелоИзДвоичныхДанных (SetBodyFromBinaryData) - Дв данные
        +-УстановитьТелоИзСтроки (SetBodyFromString) - Строка
    */

    [ContextClass("HTTPСервисОтвет", "HTTPServiceResponse")]
    public class HTTPServiceResponse : AutoContext<HTTPServiceResponse>
    {
        //System.Web.HttpContext context;

        ScriptEngine.HostedScript.Library.MapImpl headers;
        string reason = "";
        int status_code = 200;
        byte[] body = null;
        System.Text.Encoding body_encoding = System.Text.Encoding.UTF8;


        public byte[] Body
        {
            get
            {
                return body;
            }
        }

        public HTTPServiceResponse()
        {
            headers = new HostedScript.Library.MapImpl();
        }

        #region Свойства 1C

        [ContextProperty("Заголовки", "Headers")]
        public MapImpl Headers
        {
            get
            {
                return headers;
            }
            set
            {
                headers = value;
            }
        }

        [ContextProperty("Причина", "Reason")]
        public string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }

        [ContextProperty("КодСостояния", "StatusCode")]
        public int StatusCode
        {
            get
            {
                return status_code;
            }
            set
            {
                status_code = value;
            }
        }

        #endregion

        #region Функции 1С

        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string str, IValue encoding = null, IValue useBOM = null)
        {
            if (encoding == null)
            {
                body_encoding = ScriptEngine.HostedScript.Library.TextEncodingEnum.GetEncoding(new StringValue("UTF-8"), true);
            }
            else
            {
                body_encoding = ScriptEngine.HostedScript.Library.TextEncodingEnum.GetEncoding(encoding, true);
            }

            body = body_encoding.GetBytes(str);
        }

        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinaryData")]
        public void SetBodyFromBinaryData(BinaryDataContext binaryData)
        {
            body = binaryData.Buffer;
        }

        #endregion
    }

    [ContextClass("HTTPСервисКонтекст", "HTTPServiceContext")]
    public class HTTPServiceContext : AutoContext<HTTPServiceContext>
    {
        System.Web.HttpContext context;

        HTTPServiceRequest request;
        HTTPServiceResponse response;

        public HTTPServiceContext(System.Web.HttpContext ctx)
        {
            context = ctx;
            request = new HTTPServiceRequest(context);
            response = new HTTPServiceResponse();
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
            request = new HTTPServiceRequest(context);
            response = new HTTPServiceResponse();
        }

        #region Свойства для 1C

        [ContextProperty("Запрос", "Request")]
        public HTTPServiceRequest Request
        {
            get
            {
                return request;
            }
        }

        [ContextProperty("Ответ", "Response")]
        public HTTPServiceResponse Response
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
