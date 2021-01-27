/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using ScriptEngine.Machine;
using ScriptEngine.Types;
using Xunit;

namespace OneScript.Core.Tests
{
    public class ConstructionTests
    {
        [Fact]
        public void CanCreate_With_LegacyLogic()
        {
            var f = new TypeFactory(typeof(TestContextClass));
            var ctx = new TypeActivationContext
            {
                TypeName = "SomeType"
            };
            
            var constructor = f.GetConstructor(new IValue[0]);
            var instance = (TestContextClass)constructor(ctx, new IValue[0]);
            instance.CreatedViaMethod.Should().Be("Constructor0");
        
            var args = new IValue[1]
            {
                default
            };
            
            constructor = f.GetConstructor(args);
            instance = (TestContextClass)constructor(ctx, args);
            instance.CreatedViaMethod.Should().Be("Constructor1-SomeType");
        }

        [Fact]
        public void CanCreate_With_ContextInjection()
        {
            var f = new TypeFactory(typeof(TestContextClass));
            var ctx = new TypeActivationContext
            {
                TypeName = "SomeType"
            };
            
            var args = new IValue[2]
            {
                default,
                default
            };
            var constructor = f.GetConstructor(args);
            constructor.Should().NotBeNull();
            var instance = (TestContextClass)constructor(ctx, args);
            instance.CreatedViaMethod.Should().Be("Constructor2-SomeType");
        }
    }
}