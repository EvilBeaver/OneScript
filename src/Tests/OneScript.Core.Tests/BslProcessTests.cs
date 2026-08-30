/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using FluentAssertions;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.Core.Tests
{
    /// <summary>
    /// Регистрируется как scoped, поэтому живёт ровно столько же, сколько область сервисов процесса.
    /// </summary>
    public sealed class ScopedDisposableProbe : IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public class BslProcessTests
    {
        private static ScriptEngine.ScriptingEngine CreateEngineWithScopedProbe()
        {
            var builder = DefaultEngineBuilder.Create().SetDefaultOptions();
            builder.Services.RegisterScoped<ScopedDisposableProbe>();

            var engine = builder.Build();
            engine.Initialize();

            return engine;
        }

        [Fact]
        public void ProcessReleasesItsServiceScope()
        {
            var engine = CreateEngineWithScopedProbe();

            var process = engine.NewProcess();
            var scoped = process.Services.Resolve<ScopedDisposableProbe>();

            scoped.IsDisposed.Should().BeFalse("процесс ещё работает");

            scoped.IsDisposed.Should().BeTrue("процесс владеет своей областью сервисов");
        }

        [Fact]
        public void ProcessDoesNotReleaseScopeOfAnotherProcess()
        {
            var engine = CreateEngineWithScopedProbe();

            var first = engine.NewProcess();
            var second = engine.NewProcess();

            var firstScoped = first.Services.Resolve<ScopedDisposableProbe>();
            var secondScoped = second.Services.Resolve<ScopedDisposableProbe>();

            firstScoped.Should().NotBeSameAs(secondScoped, "у каждого процесса своя область сервисов");

            secondScoped.IsDisposed.Should().BeFalse("освобождение одного процесса не трогает другой");
        }
    }
}
