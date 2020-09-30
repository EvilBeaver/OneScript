/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TestNullConversions
    {
        private dynamic Instance { get; set; }

        public TestNullConversions()
        {
            Instance = new NullConversionTestContext();
        }
        
        [Fact]
        public void CheckUndefinedIsNullOnPassingArg()
        {
            var undef = ValueFactory.Create();
            var result = (IValue)Instance.ТестIValueНеопределено(undef);

            result.Should().BeNull();
        }

        [Fact]
        public void CheckDefinedValueIsNotNullOnPassingArg()
        {
            var result = (decimal)Instance.ТестIValue(7.5);
            
            result.Should().Be(7.5m);
        }
    }
}