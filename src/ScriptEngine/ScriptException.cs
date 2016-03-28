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

        public string ModuleName
        {
            get
            {
                return _codePosition.ModuleName;
            }
            internal set
            {
                _codePosition.ModuleName = value;
            }
        }

        public string ErrorDescription
        {
            get
            {
                return base.Message;
            }
        }

        public string MessageWithoutCodeFragment
        {
            get
            {
                return String.Format("{{Модуль {0} / Ошибка в строке: {1} / {2}}}",
                    this.ModuleName,
                    this.LineNumber,
                    base.Message);
            }
        }

        public override string Message
        {
            get
            {
                var sb = new StringBuilder(MessageWithoutCodeFragment);
                sb.AppendLine("    ");
                sb.Append(Code);

                return sb.ToString();
            }
        }
    }
}
