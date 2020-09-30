/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using ScriptEngine;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ObsoleteMethodAttributeTest
    {
        private List<string> _messages;
        public ObsoleteMethodAttributeTest()
        {
            var mock = new Mock<ISystemLogWriter>();
            mock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(str => _messages.Add(str));
            
            _messages = new List<string>();
            LogWriter = mock.Object;
            SystemLogger.SetWriter(LogWriter);
        }

        private ISystemLogWriter LogWriter { get; set; }

        [Fact]
        public void TestLoggingOfObsoleteCall()
        {
            dynamic instance = new TestContextClass();
            instance.УстаревшийМетод();
            instance.ObsoleteMethod();
            instance.УстаревшийМетод();

            _messages.Should().HaveCount(1, "must be only one warning");
            _messages.Should().Contain(item =>
                item.IndexOf("УстаревшийМетод", StringComparison.InvariantCultureIgnoreCase) >= 0
                || item.IndexOf("ObsoleteMethod", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        [Fact]
        public void CallGoodMethodsHasNoWarnings()
        {
            dynamic instance = new TestContextClass();
            instance.ХорошийМетод();
            instance.GoodMethod();

            _messages.Should().BeEmpty();
        }
        
        [Fact]
        public void TestICallDeprecatedAliasAndHaveWarning()
        {
            dynamic instance = new TestContextClass();
            instance.ObsoleteAlias();

            _messages.Should().HaveCount(1)
                .And.Contain(x => x.IndexOf("ObsoleteAlias", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        [Fact]
        public void TestICallDeprecatedAliasAndHaveException()
        {
            var exceptionThrown = false;

            try
            {
                dynamic instance = new TestContextClass();
                instance.VeryObsoleteAlias();
            }
            catch (RuntimeException)
            {
                exceptionThrown = true;
            }
            
            exceptionThrown.Should().BeTrue("Безнадёжно устаревший метод должен вызвать исключение");
        }
    }
}