/*----------------------------------------------------------
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
using OneScript.Exceptions;
using OneScript.Language;
using OneScript.Localization;
using OneScript.Types;
using ScriptEngine.Types;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Класс позволяет узнать информацию о произошедшем исключении.
    /// </summary>
    [ContextClass("ИнформацияОбОшибке", "ErrorInfo", TypeUUID = "E0EDED59-D37A-42E7-9796-D6C061934B5D")]
    public class ExceptionInfoContext : AutoContext<ExceptionInfoContext>
    {
        private static readonly TypeDescriptor ObjectType = typeof(ExceptionInfoContext).GetTypeFromClassMarkup();
        
        private ScriptException _exc;
        private IValue _innerException;

        public ExceptionInfoContext(ScriptException source) : base(ObjectType)
        {
            SetActualException(source);
        }

        private ExceptionInfoContext(string message, IValue parameters) : base(ObjectType)
        {
            Description = message;
            Parameters = parameters;
        }

        public bool IsErrorTemplate => _exc == null;

        private void SetActualException(ScriptException exception)
        {
            _exc = exception ?? throw new ArgumentNullException();
            Description = _exc.ErrorDescription;
            if (exception is ParametrizedRuntimeException pre)
            {
                Parameters = pre.Parameter;
            }
        }

        private ScriptException ActualException()
        {
            if (IsErrorTemplate)
            {
                throw new RuntimeException(BilingualString.Localize(
                    "Эта ИнформацияОбОшибке еще не была выброшена оператором ВызватьИсключение",
                    "This ErrorInfo is not have been thrown by Raise operator yet"));
            }

            return _exc;
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
        public string Description { get; private set;  }

        public string MessageWithoutCodeFragment => ActualException().MessageWithoutCodeFragment;

        public string GetDetailedDescription()
        {
            var exc = ActualException();
            var sb = new StringBuilder(exc.Message);
            var inner = exc.InnerException;
            while (inner != default)
            {
                sb.AppendLine();
                sb.AppendLine(Locale.NStr("ru = 'по причине:';en = 'caused by:'"));
                sb.AppendLine(inner.Message);
                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Имя модуля, вызвавшего исключение.
        /// </summary>
        [ContextProperty("ИмяМодуля", "ModuleName")]
        public string ModuleName => ActualException().ModuleName ?? string.Empty;

        /// <summary>
        /// Номер строки, вызвавшей исключение.
        /// </summary>
        [ContextProperty("НомерСтроки", "LineNumber")]
        public int LineNumber => ActualException().LineNumber;

        /// <summary>
        /// Строка исходного кода, вызвавшего исключение.
        /// </summary>
        [ContextProperty("ИсходнаяСтрока", "SourceLine")]
        public string SourceLine => ActualException().Code ?? string.Empty;

        /// <summary>
        /// Предоставляет доступ к стеку вызовов процедур.
        /// Подробнее см. класс КоллекцияКадровСтекаВызовов
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ПолучитьСтекВызовов", "GetStackTrace")]
        public IValue GetStackTrace()
        {
            if (ActualException().RuntimeSpecificInfo is IList<ExecutionFrameInfo> frames)
            {
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
            var exc = ActualException();
            if (exc.InnerException == null)
                return ValueFactory.Create();

            var alreadyWrapped = ActualException() is ExternalSystemException;
            if (!alreadyWrapped)
            {
                var inner = exc.InnerException as ScriptException ?? new ExternalSystemException(exc.InnerException);
                inner.ModuleName ??= exc.ModuleName;
                inner.Code ??= exc.Code;
                return new ExceptionInfoContext(inner);
            }
            else
            {
                if (exc.InnerException.InnerException == null)
                    return ValueFactory.Create();

                var inner = new ExternalSystemException(exc.InnerException.InnerException);
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
            return ActualException().ToString();
        }

        public override string ToString()
        {
            return Description;
        }


        [ScriptConstructor(Name = "С возможностью передачи параметров")]
        public static ExceptionInfoContext Create(IValue msg, IValue parameter)
        {
            return new ExceptionInfoContext(msg.AsString(), parameter);
        }

        public static ExceptionInfoContext EmptyExceptionInfo()
        {
            return new ExceptionInfoContext(EmptyScriptException.Instance);
        }
        
        private class EmptyScriptException : ScriptException
        {
            public static readonly EmptyScriptException Instance = new EmptyScriptException();
            private EmptyScriptException() : base("")
            {
                LineNumber = 0;
                ColumnNumber = 0;
            }

            public override string Message => "";

            public override string ToString() => "";
        }
    }
}
