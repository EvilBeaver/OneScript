/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using OneScript.Values;
using Xunit;
using FluentAssertions;
using ScriptEngine.Machine;

namespace OneScript.Core.Tests
{
    public class ValueReferenceTests
    {
        [Fact]
        public void TestCanReferenceSimpleValue()
        {
            var context = new TestContextClass();
            var reference = new ValueReference(() => BslStringValue.Create(context.BslProp), v => context.BslProp = v.AsString());

            context.BslProp = "HELLO";
            reference.Value.AsString().Should().Be("HELLO");
            reference.Value = BslStringValue.Create("NEWVAL");
            context.BslProp.Should().Be("NEWVAL");
        }
        
        [Fact]
        public void TestCanReferenceContextProperties()
        {
            var context = new TestContextClass();
            var referenceEn = new PropertyValueReference(context, "BslProp");
            var referenceRu = new PropertyValueReference(context, "СвойствоBsl");

            context.BslProp = "HELLO";
            referenceEn.Value.AsString().Should().Be("HELLO");
            referenceRu.Value.AsString().Should().Be("HELLO");

            // ссылки эквивалентны только если ссылаются на тот же объект
            referenceRu.Should().Be(referenceEn);
            var simpleReference = new ValueReference(() => default, v => {});
            referenceEn.Should().NotBe(simpleReference);

            referenceEn.Value = BslStringValue.Create("NEWVAL");
            referenceRu.Value.AsString().Should().Be("NEWVAL");
            context.BslProp.Should().Be("NEWVAL");
        }

        [Fact]
        public void TestCanReferenceIndexedValues()
        {
            var context = new TestContextClass();
            context.SetIndexedValue(BslNumericValue.Create(1), BslBooleanValue.True);
            context.SetIndexedValue(BslNumericValue.Create(2), BslBooleanValue.False);

            var ref1 = new IndexedValueReference(context, BslNumericValue.Create(1));
            var ref1Dup = new IndexedValueReference(context, BslNumericValue.Create(1));
            var ref2 = new IndexedValueReference(context, BslNumericValue.Create(2));

            ref1.Should().Be(ref1Dup);

            ref1.Value.Should().Be(context.GetIndexedValue(BslNumericValue.Create(1)));
            ref2.Value.Should().Be(context.GetIndexedValue(BslNumericValue.Create(2)));
            
            ref2.Value = BslUndefinedValue.Instance;
            context.GetIndexedValue(BslNumericValue.Create(2)).Should().Be(BslUndefinedValue.Instance);
            context.SetIndexedValue(BslNumericValue.Create(2), BslNumericValue.Create(10));
            ref2.Value.Should().Be(BslNumericValue.Create(10));
        }
    }
}
