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
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    [Collection("SystemLogger")]
    public class ObsoleteMembersTest : IDisposable
    {
        private readonly List<string> _messages;
        public ObsoleteMembersTest()
        {
            var mock = new Mock<ISystemLogWriter>();
            mock.Setup(x => x.Write(It.IsAny<string>()))
                .Callback<string>(str => _messages.Add(str));
            
            _messages = new List<string>();
            LogWriter = mock.Object;
            SystemLogger.SetWriter(LogWriter);
        }

        private ISystemLogWriter LogWriter { get; set; }

        public void Dispose()
        {
            // Очищаем логгер после каждого теста
            SystemLogger.Reset();
        }

        [Fact]
        public void TestLoggingOfObsoletePropAccess()
        {
            dynamic instance = new TestContextClass();
            var value = instance.ObsoleteProperty ?? "";
            instance.УстаревшееСвойство = value;

            _messages.Should().HaveCount(1, "must be only one warning");
            _messages.Should().Contain(item =>
                item.IndexOf("ObsoleteProperty", StringComparison.InvariantCultureIgnoreCase) >= 0
                || item.IndexOf("УстаревшееСвойство", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
        
        [Fact]
        public void TestLoggingOfObsoleteName()
        {
            dynamic instance = new TestContextClass();
            var value = instance.СвойствоBsl ?? ""; // обычное имя
            instance.OldBslProp = value;

            _messages.Should().HaveCount(1, "must be only one warning");
            _messages.Should().Contain(item => item.IndexOf("OldBslProp", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
        
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

        [Fact]
        public void DeprecatedEnumClassHasWarning()
        {
            var host = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .SetupEnvironment(e => e.AddAssembly(typeof(DeprecatedEnum).Assembly))
                .Build();

            var source = host.Loader.FromString("А = СтароеИмя.Значение1");
            host.GetCompilerService().Compile(source, ForbiddenBslProcess.Instance);
            
            _messages.Should().HaveCount(1)
                .And.Contain(x => x.Contains("СтароеИмя", StringComparison.InvariantCultureIgnoreCase));
        }
        
        [Fact]
        public void DeprecatedEnumClassHasNoWarningOnNewName()
        {
            var host = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .SetupEnvironment(e => e.AddAssembly(typeof(DeprecatedEnum).Assembly))
                .Build();

            var source = host.Loader.FromString("А = НовоеПеречисление.Значение1");
            host.GetCompilerService().Compile(source, ForbiddenBslProcess.Instance);
            
            _messages.Should().HaveCount(0);
        }
        
        [Fact]
        public void DeprecatedEnumValueHasWarning()
        {
            var env = new RuntimeEnvironment();
            var discoverer = new ContextDiscoverer(new DefaultTypeManager(), Mock.Of<IGlobalsManager>(), new TinyIocImplementation());
            discoverer.DiscoverGlobalContexts(env, GetType().Assembly, t => t == typeof(DeprecatedEnum));

            var enumInstance = env.GetGlobalProperty("НовоеПеречисление") as ClrEnumWrapper<DeprecatedEnum>;
            
            enumInstance.Should().NotBeNull();
            enumInstance["Значение1"].Should().BeAssignableTo<EnumerationValue>();
            enumInstance["Значение2"].Should().BeAssignableTo<EnumerationValue>();
            enumInstance["СтароеЗначение2"].Should().BeAssignableTo<EnumerationValue>();
            enumInstance["СтароеЗначение2"].Should().BeAssignableTo<EnumerationValue>();
            
            _messages.Should().HaveCount(1)
                .And.Contain(x => x.Contains("СтароеЗначение2", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}