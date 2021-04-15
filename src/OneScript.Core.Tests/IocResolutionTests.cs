/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using FluentAssertions;
using OneScript.DependencyInjection;
using ScriptEngine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class IocResolutionTests
    {
        private class TestService{}
        
        [Fact]
        public void Scoped_Gets_Same_Instance_As_Global()
        {
            var privateType = typeof(ScriptingEngine).Assembly.GetTypes()
                .First(t => t.Name == "TinyIocImplementation");

            privateType.Should().NotBeNull();

            var s = (IServiceDefinitions)Activator.CreateInstance(privateType);
            
            s.RegisterSingleton<TestService>();

            var parent = s.CreateContainer();
            var child = parent.CreateScope();

            var parentInstance = parent.Resolve<TestService>();
            var childInstance = child.Resolve<TestService>();

            parentInstance.Should().BeSameAs(childInstance);
        }
    }
}