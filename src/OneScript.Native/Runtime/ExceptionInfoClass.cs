/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;
using OneScript.Types;
using OneScript.Values;

namespace OneScript.Native.Runtime
{
    public class ExceptionInfoClass : BslObjectValue
    {
        private static TypeDescriptor type = new TypeDescriptor(
            new Guid("9783123B-DF0D-4821-A2B9-A9244A3530F5"),
            "ИнформацияОбОшибке",
            "ErrorInfo",
            typeof(ExceptionInfoClass));

        public static TypeDescriptor LanguageType => type;

        private ErrorPositionInfo _errorPosition;
        private Exception _exc;
        private Lazy<ExceptionInfoClass> _innerException;
        
        public ExceptionInfoClass(ScriptException lineAwareException)
            : this(lineAwareException, lineAwareException.GetPosition())
        {
        }
        
        public ExceptionInfoClass(Exception externalException, ErrorPositionInfo position)
        {
            _errorPosition = position;
            _exc = externalException;
            _innerException = new Lazy<ExceptionInfoClass>(WrapInnerException);
        }
        
        public override TypeDescriptor SystemType => type;
        
        /// <summary>
        /// Содержит краткое описание ошибки. Эквивалент Exception.Message в C#
        /// </summary>
        [ContextProperty("Описание", "Description")]
        public string Description =>
            _exc switch
            {
                ScriptException s => s.MessageWithoutCodeFragment,
                _ => _exc.Message
            };
        
        /// <summary>
        /// Имя модуля, вызвавшего исключение.
        /// </summary>
        [ContextProperty("ИмяМодуля", "ModuleName")]
        public string ModuleName => _errorPosition.ModuleName ?? string.Empty;

        /// <summary>
        /// Номер строки, вызвавшей исключение.
        /// </summary>
        [ContextProperty("НомерСтроки", "LineNumber")]
        public int LineNumber => _errorPosition.LineNumber;

        /// <summary>
        /// Строка исходного кода, вызвавшего исключение.
        /// </summary>
        [ContextProperty("ИсходнаяСтрока", "SourceLine")]
        public string SourceLine => _errorPosition.Code ?? string.Empty;
        
        /// <summary>
        /// Содержит вложенное исключение, если таковое было. Эквивалент Exception.InnerException в C#
        /// </summary>
        [ContextProperty("Причина", "Cause")]
        public ExceptionInfoClass InnerException => _innerException.Value;

        private ExceptionInfoClass WrapInnerException()
        {
            if (_exc.InnerException == null)
                return null;

            var position = _exc.InnerException is ScriptException scr ? scr.GetPosition() : _errorPosition;

            return new ExceptionInfoClass(_exc.InnerException, position);
        }
    }
}