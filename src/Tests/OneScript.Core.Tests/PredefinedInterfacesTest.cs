/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Localization;
using Xunit;

namespace OneScript.Core.Tests;

public class PredefinedInterfacesTest
{
    [Fact]
    public void Test_CheckerReactsOnModuleAnnotations()
    {
        var annotationToSearch = new BilingualString("MyAnno");
        var checker = new Mock<IPredefinedInterfaceChecker>();
        checker.Setup(x => x.GetRegistrations()).Returns(new[]
        {
            PredefinedInterfaceRegistration.OnModule(annotationToSearch),
        });

        var module = MockModule(mock =>
        {
            mock.SetupGet(x => x.ModuleAttributes).Returns(new[]
            {
                new BslAnnotationAttribute("MyAnno")
            });

            mock.SetupGet(x => x.Methods).Returns(Array.Empty<BslMethodInfo>());
        });
        
        checker.Setup(x => x.Validate(module)).Callback<IExecutableModule>((m) =>
        {
            m.Interfaces.Add(typeof(string), "Registered");
        });

        var resolver = new PredefinedInterfaceResolver(new[] { checker.Object });
        resolver.Resolve(module);

        module.Interfaces.Should().Contain(KeyValuePair.Create(typeof(string), (object)"Registered"));
    }
    
    [Fact]
    public void Test_CheckerDoesNotReactsOnModuleAnnotations()
    {
        var annotationToSearch = new BilingualString("MyAnno");
        var checker = new Mock<IPredefinedInterfaceChecker>();
        checker.Setup(x => x.GetRegistrations()).Returns(new[]
        {
            PredefinedInterfaceRegistration.OnModule(annotationToSearch),
        });

        var module = MockModule(mock =>
        {
            mock.SetupGet(x => x.ModuleAttributes).Returns(Array.Empty<BslAnnotationAttribute>());
            mock.SetupGet(x => x.Methods).Returns(Array.Empty<BslMethodInfo>());
        });
        
        checker.Setup(x => x.Validate(module)).Callback<IExecutableModule>((m) =>
        {
            m.Interfaces.Add(typeof(string), "Registered");
        });

        var resolver = new PredefinedInterfaceResolver(new[] { checker.Object });
        resolver.Resolve(module);

        module.Interfaces.Should().BeEmpty();
    }
    
    [Fact]
    public void Test_CheckerReactsOnlyOnceOnManyAnnotations()
    {
        var annotationToSearch = new BilingualString("MyAnno");
        var checker = new Mock<IPredefinedInterfaceChecker>();
        checker.Setup(x => x.GetRegistrations()).Returns(new[]
        {
            PredefinedInterfaceRegistration.OnModule(annotationToSearch),
            PredefinedInterfaceRegistration.OnMethod(annotationToSearch, new BilingualString("MyMethod")),
        });

        var module = MockModule(mock =>
        {
            mock.SetupGet(x => x.ModuleAttributes).Returns(new[]
            {
                new BslAnnotationAttribute("MyAnno")
            });

            var method = BslMethodBuilder.Create()
                .Name("MyMethod")
                .SetAnnotations(new[] { new BslAnnotationAttribute("MyAnno") })
                .Build();
            
            mock.SetupGet(x => x.Methods).Returns(new BslMethodInfo[] { method });
        });
        
        checker.Setup(x => x.Validate(module)).Verifiable();

        var resolver = new PredefinedInterfaceResolver(new[] { checker.Object });
        resolver.Resolve(module);

        checker.Verify(x => x.Validate(It.IsAny<IExecutableModule>()), Times.Once);
    }
    
    [Fact]
    public void Test_CheckerReactsOnlyOnMethodAnnotations()
    {
        var annotationToSearch = new BilingualString("MyAnno");
        var checker = new Mock<IPredefinedInterfaceChecker>();
        checker.Setup(x => x.GetRegistrations()).Returns(new[]
        {
            PredefinedInterfaceRegistration.OnMethod(annotationToSearch, new BilingualString("MyMethod")),
        });

        var module = MockModule(mock =>
        {
            var method = BslMethodBuilder.Create()
                .Name("MyMethod")
                .SetAnnotations(new[] { new BslAnnotationAttribute("MyAnno") })
                .Build();
            
            mock.SetupGet(x => x.Methods).Returns(new BslMethodInfo[] { method });
        });
        
        checker.Setup(x => x.Validate(module)).Callback<IExecutableModule>((m) =>
        {
            m.Interfaces.Add(typeof(string), "Registered");
        });

        var resolver = new PredefinedInterfaceResolver(new[] { checker.Object });
        resolver.Resolve(module);

        module.Interfaces.Should().Contain(KeyValuePair.Create(typeof(string), (object)"Registered"));
    }
    
    private static IExecutableModule MockModule(Action<Mock<IExecutableModule>> setup)
    {
        var dict = new Dictionary<Type, object>();
        var moduleMock = new Mock<IExecutableModule>();
        moduleMock.SetupGet(x => x.Interfaces).Returns(dict);

        setup(moduleMock);
        
        return moduleMock.Object;
    }
        
}