using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    class ShowUsageBehavior : AppBehavior
    {
        public override int Execute()
        {
            Console.WriteLine("1Script Execution Engine. Version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("I. Script execution: oscript.exe <script_path> [script arguments..]");
            Console.WriteLine();
            Console.WriteLine("II. Special mode: oscript.exe <mode> <script_path> [script arguments..]");
            Console.WriteLine("Mode can be one of these:");
            Console.WriteLine("  {0,-11}measures execution time", "-measure");
            Console.WriteLine("  {0,-11}shows compiled module without execution", "-compile");
            Console.WriteLine();
            Console.WriteLine("III. Build standalone executable: oscript.exe -make <script_path> <output_exe>");
            Console.WriteLine("Builds a standalone executable module based on script specified");

            return 0;
        }
    }
}
