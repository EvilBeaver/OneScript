/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OneScript.DependencyInjection;
using ScriptEngine;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.Core.Tests
{
    public class IocResolutionTests
    {
        private class TestService{}
        
        [Fact]
        public void Scoped_Gets_Same_Instance_As_Global()
        {
            var services = new ServiceCollection();
            services.AddSingleton<TestService>();

            var container = new TinyIocImplementation();
            ServiceCollectionAdapter.PopulateContainer(services, container);
            
            var parent = container;
            var child = parent.CreateScope();

            var parentInstance = parent.Resolve<TestService>();
            var childInstance = child.Resolve<TestService>();

            parentInstance.Should().BeSameAs(childInstance);
        }
    }
}