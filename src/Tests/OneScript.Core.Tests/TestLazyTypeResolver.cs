/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using FluentAssertions;
using OneScript.Exceptions;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TestLazyTypeResolver
    {
        private sealed class CountingResolver : ILazyTypeResolver
        {
            public int CallCount { get; private set; }

            public bool TryResolve(string typeName, ITypeManager typeManager, out TypeDescriptor type)
            {
                CallCount++;
                type = default;
                return false;
            }
        }

        private sealed class PrefixTestResolver : ILazyTypeResolver
        {
            public bool TryResolve(string typeName, ITypeManager typeManager, out TypeDescriptor type)
            {
                if (!typeName.StartsWith("Test.", StringComparison.Ordinal))
                {
                    type = default;
                    return false;
                }

                type = typeManager.RegisterType(typeName, default, typeof(BslNumericValue));
                return true;
            }
        }

        private sealed class FirstMatchResolver : ILazyTypeResolver
        {
            public bool TryResolve(string typeName, ITypeManager typeManager, out TypeDescriptor type)
            {
                if (typeName != "Test.First")
                {
                    type = default;
                    return false;
                }

                type = typeManager.RegisterType(typeName, default, typeof(BslNumericValue));
                return true;
            }
        }

        private sealed class StubResolver : ILazyTypeResolver
        {
            public bool TryResolve(string typeName, ITypeManager typeManager, out TypeDescriptor type)
            {
                if (typeName != "Stub.Foo")
                {
                    type = default;
                    return false;
                }

                type = typeManager.RegisterType(typeName, default, typeof(BslNumericValue));
                return true;
            }
        }

        [Fact]
        public void TryGetType_UnknownName_WithoutResolvers_ReturnsFalse()
        {
            var tm = new DefaultTypeManager();

            tm.TryGetType("Unknown.Type", out _).Should().BeFalse();
        }

        [Fact]
        public void TryGetType_WithResolverReturningFalse_BehavesAsWithoutResolvers()
        {
            var tm = new DefaultTypeManager(new ILazyTypeResolver[] { new CountingResolver() });

            tm.TryGetType("Unknown.Type", out _).Should().BeFalse();
        }

        [Fact]
        public void TryGetType_WithPrefixResolver_RegistersAndReturnsSameDescriptor()
        {
            var tm = new DefaultTypeManager(new ILazyTypeResolver[] { new PrefixTestResolver() });

            tm.TryGetType("Test.LazyType", out var first).Should().BeTrue();
            tm.TryGetType("Test.LazyType", out var second).Should().BeTrue();

            second.Should().BeSameAs(first);
        }

        [Fact]
        public void GetTypeByName_WithPrefixResolver_ReturnsSameDescriptor_OrThrowsForUnknown()
        {
            var tm = new DefaultTypeManager(new ILazyTypeResolver[] { new PrefixTestResolver() });

            tm.TryGetType("Test.LazyType", out var fromTryGet).Should().BeTrue();
            var fromGetByName = tm.GetTypeByName("Test.LazyType");

            fromGetByName.Should().BeSameAs(fromTryGet);

            Action act = () => tm.GetTypeByName("Totally.Unknown.Type");
            act.Should().Throw<RuntimeException>()
                .Which.Message.Should().Contain("Totally.Unknown.Type");
        }

        [Fact]
        public void TryGetType_WithTwoResolvers_UsesFirstMatchOnly()
        {
            var second = new CountingResolver();
            var tm = new DefaultTypeManager(new ILazyTypeResolver[]
            {
                new FirstMatchResolver(),
                second
            });

            tm.TryGetType("Test.First", out _).Should().BeTrue();
            second.CallCount.Should().Be(0);
        }

        [Fact]
        public void TryGetType_ForKnownType_DoesNotCallResolvers()
        {
            var counter = new CountingResolver();
            var tm = new DefaultTypeManager(new ILazyTypeResolver[] { counter });

            tm.TryGetType("Число", out _).Should().BeTrue();
            counter.CallCount.Should().Be(0);
        }

        [Fact]
        public void RegisterTypeDescriptor_DoesNotInvokeResolvers_AndResolverRegisterTypeIsReentrant()
        {
            var counter = new CountingResolver();
            var tm = new DefaultTypeManager(new ILazyTypeResolver[] { counter, new PrefixTestResolver() });

            var descriptor = new TypeDescriptor(typeof(BslNumericValue), "MyCustomType");
            tm.RegisterType(descriptor);
            counter.CallCount.Should().Be(0);

            tm.TryGetType("Test.Reentrant", out var resolved).Should().BeTrue();
            tm.TryGetType("Test.Reentrant", out var again).Should().BeTrue();
            again.Should().BeSameAs(resolved);
        }

        [Fact]
        public void DiIntegration_ResolvesRegisteredLazyTypeResolver()
        {
            var withoutResolver = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            withoutResolver.TypeManager.TryGetType("Stub.Foo", out _).Should().BeFalse();

            var withResolver = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .SetupServices(services => services.RegisterEnumerable<ILazyTypeResolver, StubResolver>())
                .Build();

            withResolver.TypeManager.TryGetType("Stub.Foo", out var type).Should().BeTrue();
            type.Name.Should().Be("Stub.Foo");
        }
    }
}
