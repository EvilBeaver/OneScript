/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using FluentAssertions;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Native.Runtime;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Dynamic.Tests;

public class DynamicOperationsTest
{
    [Theory]
    [MemberData(nameof(ArgsForEqualityOperators))]
    public void DynamicEquality(BslValue left, BslValue right, bool expected)
    {
        var result = DynamicOperations.Equality(left, right);
        result.Should().Be(expected);
    }
    
    [Fact]
    public void DynamicAdditionForNulls()
    {
        Assert.Throws<TypeConversionException>(() => DynamicOperations.Add(null, null));
    }
    
    [Fact]
    public void DynamicSubtractionForNulls()
    {
        Assert.Throws<TypeConversionException>(() => DynamicOperations.Subtract(null, null));
    }

    [Fact]
    public void CallContextMethod_WithInjectedProcess()
    {
        var arguments = new BslValue[] { BslNumericValue.Create(1), BslNumericValue.Create(2) };

        var result = DynamicOperations.CallContextMethod(new ProcessInjectingContext(), "Сложить",
            ForbiddenBslProcess.Instance, arguments);

        result.AsNumber().Should().Be(3);
    }

    [Fact]
    public void CallContextMethod_WithInjectedProcess_TooManyArguments()
    {
        var arguments = new BslValue[] { BslNumericValue.Create(1), BslNumericValue.Create(2), BslNumericValue.Create(3) };

        Assert.Throws<RuntimeException>(() => DynamicOperations.CallContextMethod(new ProcessInjectingContext(), "Сложить",
            ForbiddenBslProcess.Instance, arguments));
    }
    
    public static IEnumerable<object[]> ArgsForEqualityOperators()
    {
        yield return new object[] { BslBooleanValue.True, BslBooleanValue.True, true };
        yield return new object[] { BslBooleanValue.True, null, false };
        yield return new object[] { null, BslBooleanValue.True, false };
        yield return new object[] { BslBooleanValue.True, BslUndefinedValue.Instance, false };
        // TODO: расширить по мере возможности
    }

    [ContextClass("ТестВнедрениеПроцесса", "TestProcessInjection")]
    private class ProcessInjectingContext : AutoContext<ProcessInjectingContext>
    {
        [ContextMethod("Сложить", "Add")]
        public int Add(IBslProcess process, int left, int right)
        {
            return left + right;
        }
    }
}