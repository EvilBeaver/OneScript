using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;

namespace Benchmarking
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<TokenizerBenchmark>();
            //var b = new TokenizerBenchmark();
            //b.RunTest();
        }
    }
}
