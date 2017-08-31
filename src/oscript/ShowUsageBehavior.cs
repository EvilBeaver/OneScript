/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace oscript
{
    internal class ShowUsageBehavior : AppBehavior
    {
        public override int Execute()
        {
            Output.WriteLine($"1Script Execution Engine. Version {Assembly.GetExecutingAssembly().GetName().Version}");
            Output.WriteLine();
            Output.WriteLine("Usage:");
            Output.WriteLine();
            Output.WriteLine("I. Script execution: oscript.exe <script_path> [script arguments..]");
            Output.WriteLine();
            Output.WriteLine("II. Special mode: oscript.exe <mode> <script_path> [script arguments..]");
            Output.WriteLine("Mode can be one of these:");
            Output.WriteLine($"  {"-measure",-12}measures execution time");
            Output.WriteLine($"  {"-compile",-12}shows compiled module without execution");
            Output.WriteLine($"  {"-check [-env=<entrypoint-file>]",-12}provides syntax check");
            Output.WriteLine($"  {"-check -cgi",-12}provides syntax check in CGI-mode");
            Output.WriteLine();
            Output.WriteLine("  -encoding=<encoding-name> set output encoding");
            Output.WriteLine("  -codestat=<filename> write code statistics");
            Output.WriteLine();
            Output.WriteLine("III. Build standalone executable: oscript.exe -make <script_path> <output_exe>");
            Output.WriteLine("  Builds a standalone executable module based on script specified");
            Output.WriteLine();
            Output.WriteLine("IV. Run as CGI application: oscript.exe -cgi <script_path> [script arguments..]");
            Output.WriteLine("  Runs as CGI application under HTTP-server (Apache/Nginx/IIS/etc...)");

            return 0;
        }
    }
}