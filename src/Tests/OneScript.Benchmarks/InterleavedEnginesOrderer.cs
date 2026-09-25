/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace OneScript.Benchmarks
{
    /// <summary>
    /// Версии движка для одного бенчмарка идут подряд, а их порядок чередуется от бенчмарка к бенчмарку.
    /// По умолчанию BenchmarkDotNet прогоняет все бенчмарки одной версии, потом следующей,
    /// и нагрев процессора или фоновая нагрузка за это время сдвигают Ratio в одну сторону.
    /// </summary>
    internal sealed class InterleavedEnginesOrderer : DefaultOrderer
    {
        public InterleavedEnginesOrderer() : base(SummaryOrderPolicy.Default, MethodOrderPolicy.Declared)
        {
        }

        public override IEnumerable<BenchmarkCase> GetExecutionOrder(
            ImmutableArray<BenchmarkCase> benchmarksCase,
            IEnumerable<BenchmarkLogicalGroupRule> order = null)
        {
            var benchmarks = base.GetExecutionOrder(benchmarksCase, order)
                .GroupBy(benchmark => (benchmark.Descriptor, benchmark.Parameters.DisplayInfo))
                .ToList();

            for (int i = 0; i < benchmarks.Count; i++)
            {
                var engines = benchmarks[i].ToList();
                if (i % 2 == 1)
                    engines.Reverse();

                foreach (var benchmark in engines)
                    yield return benchmark;
            }
        }
    }
}
