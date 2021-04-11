/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Values;
using Xunit;

namespace OneScript.Dynamic.Tests
{
    public class DefaultDlrBehaviorsTest
    {
        [Fact]
        public void Test_BinaryOperation_Binding_LeftSide()
        {
            dynamic bslNumber = BslNumericValue.Create(5);
            var result = bslNumber + 5;
            
            Assert.Equal(10, result);
        }
        
        [Fact]
        public void Test_BinaryOperation_Binding_RightSide()
        {
            dynamic bslNumber = BslNumericValue.Create(5);
            var result = 5 + bslNumber;
            
            Assert.Equal(10, result);
        }
        
        [Fact]
        public void Test_BinaryOperation_EqualityTest()
        {
            dynamic bslNumber = BslNumericValue.Create(5);
            Assert.True(bslNumber == 5);
        }
        
        [Fact]
        public void Test_BinaryOperation_EqualityTest_OfBoth()
        {
            dynamic bslNumber1 = BslNumericValue.Create(5);
            dynamic bslNumber2 = BslNumericValue.Create(5);
            
            Assert.True(bslNumber1 == bslNumber2);
            Assert.Equal(bslNumber1, bslNumber2);
        }
        
        [Fact]
        public void Test_BinaryOperation_ComparisonTest()
        {
            dynamic bslNumber1 = BslNumericValue.Create(5);
            dynamic bslNumber2 = BslNumericValue.Create(2);
            Assert.True(bslNumber1 > bslNumber2);
        }
    }
}