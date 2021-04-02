using OneScript.Values;
using Xunit;

namespace OneScript.Dynamic.Tests
{
    public class DefaultDlrBehaviorsTest
    {
        [Fact]
        public void Test_BinaryOperation_Binding_LeftSide()
        {
            dynamic bslNumber = new BslNumericValue(5);
            var result = bslNumber + 5;
            
            Assert.Equal(10, result);
        }
        
        [Fact]
        public void Test_BinaryOperation_Binding_RightSide()
        {
            dynamic bslNumber = new BslNumericValue(5);
            var result = 5 + bslNumber;
            
            Assert.Equal(10, result);
        }
        
        [Fact]
        public void Test_BinaryOperation_EqualityTest()
        {
            dynamic bslNumber = new BslNumericValue(5);
            Assert.True(bslNumber == 5);
        }
        
        [Fact]
        public void Test_BinaryOperation_EqualityTest_OfBoth()
        {
            dynamic bslNumber1 = new BslNumericValue(5);
            dynamic bslNumber2 = new BslNumericValue(5);
            
            Assert.True(bslNumber1 == bslNumber2);
            Assert.Equal(bslNumber1, bslNumber2);
        }
        
        [Fact]
        public void Test_BinaryOperation_ComparisonTest()
        {
            dynamic bslNumber1 = new BslNumericValue(5);
            dynamic bslNumber2 = new BslNumericValue(2);
            Assert.True(bslNumber1 > bslNumber2);
        }
    }
}