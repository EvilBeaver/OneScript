using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugServer
{
    class Program
    {
        static void Main(string[] args)
        {
            bool showTrace = false;

            foreach (var argument in args)
            {
                switch (argument)
                {
                    case "-trace":
                        showTrace = true;
                        break;
                }
            }

            StartSession(showTrace, Console.OpenStandardInput(), Console.OpenStandardOutput());

        }

        private static void StartSession(bool showTrace, Stream input, Stream output)
        {
            var session = new OscriptDebugSession();
            session.TRACE = showTrace;
            session.TRACE_RESPONSE = showTrace;
            session.Start(input, output).Wait();
        }
    }
}
