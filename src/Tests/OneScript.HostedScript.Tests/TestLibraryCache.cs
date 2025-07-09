using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.LibraryCache;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.HostedScript.Tests;

public class TestLibraryCache
{
    [Fact]
    public void TestChecksValidityWhenReading()
    {
        var cacheMock = new Mock<IScriptCacheStorage>();
        cacheMock.Setup(s => s.Exists(It.Is<string>(v => v == "key"))).Returns(true);
        cacheMock.Setup(s => s.IsValid(It.Is<string>(v => v == "key"))).Returns(true);
        
        var cacheService = new DefaultScriptCacheService(cacheMock.Object, 
            new OneScriptLibraryOptions(
                new KeyValueConfig(new Dictionary<string, string>(){{"lib.caching", "true"}}))
            );
        
        var readResult = cacheService.TryLoadFromCache("key", out _);
        cacheMock.Verify(s => s.IsValid(It.Is<string>(v => v == "key")), Times.Once);
        readResult.Should().BeTrue();
    }
}