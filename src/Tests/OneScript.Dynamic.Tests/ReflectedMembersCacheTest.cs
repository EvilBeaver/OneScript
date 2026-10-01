/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading.Tasks;
using FluentAssertions;
using OneScript.Native.Compiler;
using Xunit;

namespace OneScript.Dynamic.Tests;

public class ReflectedMembersCacheTest
{
    [Fact]
    public void CacheSurvivesParallelAccess()
    {
        // имен больше емкости: и попадания, и вытеснение
        var cache = new ReflectedMethodsCache(4);
        var names = new[] { "Sin", "Cos", "Tan", "Sqrt", "Exp", "Log10" };

        Action act = () => Parallel.For(0, 8, _ =>
        {
            for (var i = 0; i < 100000; i++)
            {
                var name = names[i % names.Length];
                if (cache.GetOrAdd(typeof(Math), name).Name != name)
                    throw new InvalidOperationException($"Wrong method for {name}");
            }
        });

        act.Should().NotThrow();
    }
}
