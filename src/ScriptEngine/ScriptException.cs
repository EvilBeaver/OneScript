using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine
{
    public class ScriptException : ApplicationException
    {
        private CodePositionInfo _codePosition;

        internal ScriptException() 
        {
            _codePosition = new CodePositionInfo();
            _codePosition.LineNumber = -1;
        }

        internal ScriptException(string message)
            : this(new CodePositionInfo(), message, null)
        {

        }

        internal ScriptException(CodePositionInfo codeInfo, string message)
            : this(codeInfo, message, null)
        {

        }

        internal ScriptException(CodePositionInfo codeInfo, string message, Exception innerException)
            : base(message, innerException)
        {
            _codePosition = codeInfo;
        }

        public int LineNumber
        {
            get
            {
                return _codePosition.LineNumber;
            }
            internal set
            {
                _codePosition.LineNumber = value;
            }
        }

        public string Code
        {
            get
            {
                return _codePosition.Code;
            }
            internal set
            {
                _codePosition.Code = value;
            }
        }

        public override string Message
        {
            get
            {
                return String.Format("{{Ошибка в строке {0}: {1}}}\n  {2}",
                    _codePosition.LineNumber,
                    base.Message,
                    _codePosition.Code);
            }
        }
    }
}
