/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.Binary;

/// <summary>
/// 
/// </summary>

namespace ScriptEngine.HostedScript.Library.HTTPService
{
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
        MapImpl _headers = new MapImpl();
        string _reason = "";
        int _statusCode = 200;
        string _contentCharset = Encoding.UTF8.WebName;

        Stream _bodyStream = new MemoryStream();

        public Stream BodyStream => _bodyStream;
        public string ContentCharset => _contentCharset;

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
            if ((_bodyStream as FileStream) == null)
                return ValueFactory.Create();
            else
                return ValueFactory.Create(((FileStream)_bodyStream).Name);
        }

        [ContextMethod("ПолучитьТелоКакДвоичныеДанные", "GetBodyAsBinaryData")]
        public BinaryDataContext ПолучитьТелоКакДвоичныеДанные()
        {
            if ((_bodyStream as MemoryStream) == null)
                return null;
            else
                return new BinaryDataContext(((MemoryStream)_bodyStream).GetBuffer());
        }

        [ContextMethod("ПолучитьТелоКакПоток", "GetBodyAsStream")]
        public GenericStream GetBodyAsStream()
        {
            return new GenericStream(_bodyStream);
        }

        [ContextMethod("ПолучитьТелоКакСтроку", "GetBodyAsString")]
        public IValue GetBodyAsString()
        {
            var body = _bodyStream as MemoryStream;
            if (body == null)
                return ValueFactory.Create();
            else
            {
                // Выяснено экспериментальным путем, используется UTF8 (8.3.10.2650)
                return ValueFactory.Create(Encoding.UTF8.GetString(body.GetBuffer(), 0, (int)body.Length));
            }
        }

        [ContextMethod("УстановитьИмяФайлаТела", "SetBodyFileName")]
        public void SetBodyFileName(IValue fileName)
        {
            _contentCharset = Encoding.UTF8.WebName;
            _bodyStream = new FileStream(fileName.AsString(), FileMode.Open, FileAccess.Read);
        }

        [ContextMethod("УстановитьТелоИзДвоичныхДанных", "SetBodyFromBinaryData")]
        public void SetBodyFromBinaryData(BinaryDataContext binaryData)
        {
            _contentCharset = Encoding.UTF8.WebName;
            _bodyStream = new MemoryStream();
            _bodyStream.Write(binaryData.Buffer, 0, binaryData.Buffer.Length);
            _bodyStream.Seek(0, SeekOrigin.Begin);
        }

        [ContextMethod("УстановитьТелоИзСтроки", "SetBodyFromString")]
        public void SetBodyFromString(string str, IValue encoding = null, IValue useBOM = null)
        {
            // Получаем кодировку
            // useBOM должен иметь тип ИспользованиеByteOrderMark он не реализован. Его не используем
            // Из синтаксис-помощника в режиме совместимости Использовать
            // Из синтаксис помощника если кодировка не задана используем UTF8

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);

            _contentCharset = enc.WebName;

            _bodyStream = new MemoryStream();
            byte[] buffer = enc.GetBytes(str);
            _bodyStream.Write(buffer, 0, buffer.Length);
            _bodyStream.Seek(0, SeekOrigin.Begin);
        }

        [ScriptConstructor(Name = "По коду состояния, причине и заголовкам")]
        public static HTTPServiceResponseImpl Constructor(IValue statusCode, IValue reason = null, MapImpl headers = null)
        {
            var response = new HTTPServiceResponseImpl();

            response._statusCode = Convert.ToInt16(statusCode.AsNumber());

            if (reason != null)
                response._reason = reason.AsString();

            if (headers != null)
                response._headers = headers;

            return response;
        }

        #endregion
    }
}