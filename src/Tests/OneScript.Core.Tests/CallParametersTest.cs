/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Values;
using Xunit;

namespace OneScript.Core.Tests
{
    public class CallParametersTest
    {
        [Fact]
        public void ContextMethod_CallParameters_SkipProcess_And_KeepFlags()
        {
            var method = new ContextMethodInfo(typeof(TestContext).GetMethod(nameof(TestContext.Method)));

            var parameters = method.CallParameters.ToArray();

            parameters.Should().HaveCount(3);
            parameters[0].IsByRef.Should().BeTrue();
            parameters[0].HasDefaultValue.Should().BeFalse();
            parameters[1].IsByRef.Should().BeFalse();
            parameters[1].HasDefaultValue.Should().BeFalse();
            parameters[2].IsByRef.Should().BeFalse();
            parameters[2].HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void ScriptMethod_CallParameters_KeepFlags()
        {
            var builder = BslMethodBuilder.Create().Name("Method");
            builder.NewParameter().Name("ByRef");
            builder.NewParameter().Name("ByVal").ByValue(true);
            builder.NewParameter().Name("Optional").ByValue(true).DefaultValue(BslBooleanValue.True);
            var method = builder.Build();

            var parameters = method.CallParameters.ToArray();

            parameters.Should().HaveCount(3);
            parameters[0].IsByRef.Should().BeTrue();
            parameters[0].HasDefaultValue.Should().BeFalse();
            parameters[1].IsByRef.Should().BeFalse();
            parameters[1].HasDefaultValue.Should().BeFalse();
            parameters[2].IsByRef.Should().BeFalse();
            parameters[2].HasDefaultValue.Should().BeTrue();
        }

        [Fact]
        public void CallParameters_AreCached()
        {
            var method = new ContextMethodInfo(typeof(TestContext).GetMethod(nameof(TestContext.Method)));

            var first = method.CallParameters;
            var second = method.CallParameters;

            (first == second).Should().BeTrue();
        }

        private class TestContext
        {
            [ContextMethod("Метод", "Method")]
            public void Method(IBslProcess process, [ByRef] IVariable byRef, int required, string optional = null)
            {
            }
        }
    }
}
