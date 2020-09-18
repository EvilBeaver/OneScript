/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;

namespace OneScript.Language
{
    public class ScriptException : ApplicationException
    {
        private readonly ErrorPositionInfo _codePosition;

        public ScriptException() 
        {
            _codePosition = new ErrorPositionInfo();
            _codePosition.LineNumber = -1;
        }

        public ScriptException(string message)
            : this(new ErrorPositionInfo(), message, null)
        {

        }

        public ScriptException(ErrorPositionInfo errorInfo, string message)
            : this(errorInfo, message, null)
        {

        }

        public ScriptException(ErrorPositionInfo errorInfo, string message, Exception innerException)
            : base(message, innerException)
        {
            _codePosition = errorInfo;
        }

        public int LineNumber
        {
            get => _codePosition.LineNumber;
            set => _codePosition.LineNumber = value;
        }

        public int ColumnNumber
        {
            get => _codePosition.ColumnNumber;
            set => _codePosition.ColumnNumber = value;
        }

        public string Code
        {
            get => _codePosition.Code;
            set => _codePosition.Code = value;
        }

        public string ModuleName
        {
            get => _codePosition.ModuleName;
            set => _codePosition.ModuleName = value;
        }

        public string ErrorDescription => base.Message;

        public string MessageWithoutCodeFragment
        {
            get
            {
                if (ColumnNumber != ErrorPositionInfo.OUT_OF_TEXT)
                    return $"{{Модуль {ModuleName} / Ошибка в строке: {LineNumber},{ColumnNumber} / {base.Message}}}";
                
                return $"{{Модуль {ModuleName} / Ошибка в строке: {LineNumber} / {base.Message}}}";
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
