﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Language;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Класс позволяет узнать информацию о произошедшем исключении.
    /// </summary>
    [ContextClass("ИнформацияОбОшибке", "ErrorInfo")]
    public class ExceptionInfoContext : AutoContext<ExceptionInfoContext>
    {
        readonly ScriptException _exc;
        IValue _innerException;

        public ExceptionInfoContext(ScriptException source)
        {
            _exc = source ?? throw new ArgumentNullException();
            if (source.InnerException is ParametrizedRuntimeException pre)
            {
                Parameters = pre.Parameter;
            }
        }
        
        /// <summary>
        /// Значение, переданное при создании исключения в конструкторе объекта ИнформацияОбОшибке.
        /// </summary>
        [ContextProperty("Параметры", "Parameters", CanWrite = false)]
        public IValue Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Содержит краткое описание ошибки. Эквивалент Exception.Message в C#
        /// </summary>
        [ContextProperty("Описание", "Description")]
        public string Description 
        { 
            get { return _exc.ErrorDescription; } 
        }

        public string MessageWithoutCodeFragment
        {
            get { return _exc.MessageWithoutCodeFragment; }
        }

        public string DetailedDescription
        {
            get
            {
                var sb = new StringBuilder(_exc.Message);
                var inner = _exc.InnerException;
                while (inner != default)
                {
                    sb.AppendLine();
                    sb.AppendLine(Locale.NStr("ru = 'по причине:';en = 'caused by:'"));
                    sb.AppendLine(inner.Message);
                    inner = inner.InnerException;
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Имя модуля, вызвавшего исключение.
        /// </summary>
        [ContextProperty("ИмяМодуля", "ModuleName")]
        public string ModuleName => _exc.ModuleName ?? string.Empty;

        /// <summary>
        /// Номер строки, вызвавшей исключение.
        /// </summary>
        [ContextProperty("НомерСтроки", "LineNumber")]
        public int LineNumber => _exc.LineNumber;

        /// <summary>
        /// Строка исходного кода, вызвавшего исключение.
        /// </summary>
        [ContextProperty("ИсходнаяСтрока", "SourceLine")]
        public string SourceLine => _exc.Code ?? string.Empty;

        /// <summary>
        /// Предоставляет доступ к стеку вызовов процедур.
        /// Подробнее см. класс КоллекцияКадровСтекаВызовов
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ПолучитьСтекВызовов", "GetStackTrace")]
        public IValue GetStackTrace()
        {
            if (_exc.RuntimeSpecificInfo is IList<ExecutionFrameInfo> frames)
            {
                // var frames = rte.CallStackFrames;
                // if (frames == null)
                //    return ValueFactory.Create();
                return new StackTraceCollectionContext(frames);
            }
            return ValueFactory.Create();
        }

        /// <summary>
        /// Содержит вложенное исключение, если таковое было. Эквивалент Exception.InnerException в C#
        /// </summary>
        [ContextProperty("Причина", "Cause")]
        public IValue InnerException
        {
            get 
            {
                if (_innerException == null)
                    _innerException = CreateInnerExceptionInfo();

                return _innerException;
            }
        }

        private IValue CreateInnerExceptionInfo()
        {
            if (_exc.InnerException == null)
                return ValueFactory.Create();

            bool alreadyWrapped = _exc is ExternalSystemException;
            if (!alreadyWrapped)
            {
                ScriptException inner;
                inner = _exc.InnerException as ScriptException;
                if (inner == null)
                {
                    inner = new ExternalSystemException(_exc.InnerException);
                }
                if (inner.ModuleName == null)
                    inner.ModuleName = _exc.ModuleName;
                if (inner.Code == null)
                    inner.Code = _exc.Code;
                return new ExceptionInfoContext(inner);
            }
            else
            {
                if (_exc.InnerException.InnerException == null)
                    return ValueFactory.Create();

                var inner = new ExternalSystemException(_exc.InnerException.InnerException);
                if (inner.LineNumber == ErrorPositionInfo.OUT_OF_TEXT)
                {
                    inner.ModuleName = this.ModuleName;
                    inner.Code = this.SourceLine;
                    inner.LineNumber = this.LineNumber;
                }

                return new ExceptionInfoContext(inner);
            }
        }

        /// <summary>
        /// Содержит подробное описание исключения, включая стек вызовов среды исполнения CLR.
        /// т.е. не стек вызовов скрипта, а стек вызовов скриптового движка.
        /// Эквивалентно функции Exception.ToString() в C#.
        /// </summary>
        /// <returns>Строка.</returns>
        [ContextMethod("ПодробноеОписаниеОшибки", "DetailedDescription")]
        public string GetDescription()
        {
            return _exc.ToString();
        }

        public override string ToString()
        {
            return Description;
        }


        [ScriptConstructor(Name = "С возможностью передачи параметров")]
        public static ExceptionTemplate Create(IValue msg, IValue parameter)
        {
            return new ExceptionTemplate(msg.AsString(), parameter);
        }

    }
}
