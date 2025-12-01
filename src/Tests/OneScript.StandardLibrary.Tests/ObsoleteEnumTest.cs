/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using Moq;
using OneScript.Contexts;
using OneScript.Contexts.Enums;
using ScriptEngine;

namespace OneScript.StandardLibrary.Tests
{
    public class ObsoleteEnumTest
    {
        private List<string> _messages;

        public ObsoleteEnumTest()
        {
            var mock = new Mock<ISystemLogWriter>();
            mock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(str => _messages.Add(str));

            _messages = new List<string>();
            LogWriter = mock.Object;
            SystemLogger.SetWriter(LogWriter);
        }

        private ISystemLogWriter LogWriter { get; set; }

        [DeprecatedName("NonActual")]
        [EnumerationType("Актуальное", "Actual")]
        public enum TestEnum
        {
            One,
            Two
        }
    }
}