/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.StandardLibrary;
using ScriptEngine.HostedScript;

namespace NUnitTests.Bsl
{
    internal sealed class CapturingHostApplication : IHostApplication
    {
        private readonly List<BslLogMessage> _messages = new List<BslLogMessage>();

        public IReadOnlyList<BslLogMessage> Messages => _messages;

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            lock (_messages)
            {
                _messages.Add(new BslLogMessage(str ?? string.Empty, status));
            }
        }

        public void ShowExceptionInfo(Exception exc)
        {
            var message = exc?.ToString() ?? "Неизвестная ошибка";
            Echo($"Исключение: {message}", MessageStatusEnum.Important);
        }

        public bool InputString(out string result, string prompt, int maxLen, bool multiline)
        {
            result = string.Empty;
            return false;
        }

        public string[] GetCommandLineArguments()
        {
            return Array.Empty<string>();
        }

        public void ClearMessages()
        {
            _messages.Clear();
        }
    }
}

