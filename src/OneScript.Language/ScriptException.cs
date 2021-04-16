/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;

namespace OneScript.Language
{
    public class ScriptException : ApplicationException
    {
        private readonly ErrorPositionInfo _codePosition;
        
        public ScriptException(string message)
            : this(new ErrorPositionInfo(), message, null)
        {
        }

        public ScriptException(ErrorPositionInfo errorInfo, string message, Exception innerException = null)
            : base(message, innerException)
        {
            _codePosition = errorInfo ?? throw new ArgumentNullException(nameof(errorInfo));
        }

        public ScriptException(ErrorPositionInfo errorInfo, Exception innerException)
            : base(innerException.Message, innerException)
        {
            _codePosition = errorInfo ?? throw new ArgumentNullException(nameof(errorInfo));
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

        public ErrorPositionInfo GetPosition()
        {
            return _codePosition;
        }

        public string ErrorDescription => base.Message;

        public string MessageWithoutCodeFragment
        {
            get
            {
                var parts = new List<string>();

                if (!string.IsNullOrEmpty(ModuleName))
                    parts.Add($"Модуль {ModuleName}");

                if (LineNumber != ErrorPositionInfo.OUT_OF_TEXT)
                {
                    parts.Add(ColumnNumber != ErrorPositionInfo.OUT_OF_TEXT
                        ? $"Ошибка в строке: {LineNumber},{ColumnNumber}"
                        : $"Ошибка в строке: {LineNumber}");
                }
                parts.Add(base.Message);

                var unquotedResult = string.Join(" / ", parts);
                return $"{{{unquotedResult}}}";
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
        
        public object RuntimeSpecificInfo { get; set; }
    }
}
