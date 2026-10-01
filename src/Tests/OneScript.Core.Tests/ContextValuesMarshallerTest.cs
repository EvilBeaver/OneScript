/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ContextValuesMarshallerTest
    {
        [Fact]
        public void ConvertParam_Int()
        {
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create(5), 0).Should().Be(5);
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create("12"), 0).Should().Be(12);
            ContextValuesMarshaller.ConvertParam(Variable.Create(ValueFactory.Create(3), "x"), 0).Should().Be(3);
            ContextValuesMarshaller.ConvertParam(BslSkippedParameterValue.Instance, 7).Should().Be(7);
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create(), 7).Should().Be(7);
        }

        [Fact]
        public void ConvertParam_Int_Overflow()
        {
            var huge = ValueFactory.Create(100_000_000_000m);

            Assert.Throws<RuntimeException>(() => ContextValuesMarshaller.ConvertParam(huge, 0));
        }

        [Fact]
        public void ConvertParam_Primitives()
        {
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create(1.5m), 0m).Should().Be(1.5m);
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create(true), false).Should().BeTrue();
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create(1), false).Should().BeTrue();
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create("строка"), "").Should().Be("строка");
            ContextValuesMarshaller.ConvertParam(ValueFactory.Create(5), "").Should().Be("5");
        }

        [Fact]
        public void ConvertReturnValue_Primitives()
        {
            ContextValuesMarshaller.ConvertReturnValue(true).Should().BeSameAs(BslBooleanValue.True);
            ContextValuesMarshaller.ConvertReturnValue(5).AsNumber().Should().Be(5);
            ContextValuesMarshaller.ConvertReturnValue(1.5m).AsNumber().Should().Be(1.5m);
        }

        [Fact]
        public void ContextMethod_CallThroughWrapper()
        {
            Call("Повторить", ValueFactory.Create("ab"), ValueFactory.Create(2)).AsString(ForbiddenBslProcess.Instance).Should().Be("abab");
            Call("Повторить", ValueFactory.Create(5), BslSkippedParameterValue.Instance).AsString(ForbiddenBslProcess.Instance).Should().Be("5");

            Call("Удвоить", ValueFactory.Create(5)).AsNumber().Should().Be(10);
            Call("Удвоить", ValueFactory.Create("6")).AsNumber().Should().Be(12);
            Call("Удвоить", Variable.Create(ValueFactory.Create(3), "x")).AsNumber().Should().Be(6);
            Call("Удвоить", ValueFactory.Create()).AsNumber().Should().Be(14);
            Call("Удвоить", BslSkippedParameterValue.Instance).AsNumber().Should().Be(14);

            Call("Половина", ValueFactory.Create(3m)).AsNumber().Should().Be(1.5m);

            Call("Не", ValueFactory.Create(true)).Should().BeSameAs(BslBooleanValue.False);
            Call("Не", ValueFactory.Create(0)).Should().BeSameAs(BslBooleanValue.True);
        }

        [Fact]
        public void ContextMethod_CallThroughWrapper_IntOverflow()
        {
            Assert.Throws<RuntimeException>(() => Call("Удвоить", ValueFactory.Create(100_000_000_000m)));
        }

        private static IValue Call(string methodName, params IValue[] arguments)
        {
            var context = new MarshallingTestContext();
            context.CallAsFunction(context.GetMethodNumber(methodName), arguments, out var result, ForbiddenBslProcess.Instance);
            return result;
        }

        [ContextClass("ТестМаршаллинга", "MarshallingTest")]
        private class MarshallingTestContext : AutoContext<MarshallingTestContext>
        {
            [ContextMethod("Повторить", "Repeat")]
            public string Repeat(string text, int count = 1)
            {
                return string.Concat(System.Linq.Enumerable.Repeat(text, count));
            }

            [ContextMethod("Удвоить", "Twice")]
            public int Twice(int value = 7)
            {
                return value * 2;
            }

            [ContextMethod("Половина", "Half")]
            public decimal Half(decimal value)
            {
                return value / 2;
            }

            [ContextMethod("Не", "Not")]
            public bool Not(bool value)
            {
                return !value;
            }
        }
    }
}
