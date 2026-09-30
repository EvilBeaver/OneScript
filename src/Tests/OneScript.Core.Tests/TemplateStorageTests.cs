/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Moq;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.HostedScript;
using Xunit;

namespace OneScript.Core.Tests
{
    public class TemplateStorageTests
    {
        [Fact]
        public void TemplatesRegisteredFromManyThreadsAreKept()
        {
            const int threadsCount = 8;
            const int templatesPerThread = 500;
            var storage = new TemplateStorage(Mock.Of<ITemplateFactory>());
            var errors = new ConcurrentQueue<Exception>();

            // Так регистрируют макеты библиотеки, загружаемые из разных заданий
            var threads = Enumerable.Range(0, threadsCount).Select(number => new Thread(() =>
            {
                try
                {
                    for (var i = 0; i < templatesPerThread; i++)
                    {
                        storage.RegisterTemplate($"Макет{number}_{i}", new StubTemplate());
                    }
                }
                catch (Exception e)
                {
                    errors.Enqueue(e);
                }
            })).ToArray();

            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }

            errors.Should().BeEmpty();
            storage.GetTemplates().Should().HaveCount(threadsCount * templatesPerThread);
        }

        private class StubTemplate : ITemplate
        {
            public string GetFilename() => "";

            public BinaryDataContext GetBinaryData() => throw new NotSupportedException();

            public TemplateKind Kind => TemplateKind.File;

            public void Dispose()
            {
            }
        }
    }
}
