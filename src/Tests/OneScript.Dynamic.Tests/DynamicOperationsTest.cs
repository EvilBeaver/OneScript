/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using FluentAssertions;
using OneScript.Exceptions;
using OneScript.Native.Runtime;
using OneScript.Values;
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
    
    public static IEnumerable<object[]> ArgsForEqualityOperators()
    {
        yield return new object[] { BslBooleanValue.True, BslBooleanValue.True, true };
        yield return new object[] { BslBooleanValue.True, null, false };
        yield return new object[] { null, BslBooleanValue.True, false };
        yield return new object[] { BslBooleanValue.True, BslUndefinedValue.Instance, false };
        // TODO: расширить по мере возможности
    }
}