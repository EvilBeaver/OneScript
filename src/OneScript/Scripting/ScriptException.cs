using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class ScriptException : EngineException
    {
        private CodePositionInfo _codePosition;

        public ScriptException() 
        {
            _codePosition = new CodePositionInfo();
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

        public int ColumnNumber
        {
            get
            {
                return _codePosition.ColumnNumber;
            }
            set
            {
                _codePosition.ColumnNumber = value;
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

        public static ScriptException AppendCodeInfo(ScriptException exc, int line, string codeString, int column = -1)
        {
            const int UNKNOWN_POSITION = -1;

            if (exc.LineNumber == UNKNOWN_POSITION)
            {
                exc.LineNumber = line;
                exc.Code = codeString;
            }

            if (column != UNKNOWN_POSITION && exc.ColumnNumber == UNKNOWN_POSITION)
            {
                exc.ColumnNumber = column;
            }

            return exc;
        }

        public static ScriptException AppendCodeInfoForced(ScriptException exc, int line, int column, string codeString)
        {
            exc.LineNumber = line;
            exc.ColumnNumber = column;
            exc.Code = codeString;
            
            return exc;
        }


        internal static ScriptException AppendCodeInfo(ScriptException exc, CodePositionInfo codePosInfo)
        {
            AppendCodeInfo(exc, codePosInfo.LineNumber, codePosInfo.Code, codePosInfo.ColumnNumber);
            return exc;
        }

    }
}
