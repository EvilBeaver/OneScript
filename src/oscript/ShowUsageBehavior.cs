/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
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
            Output.WriteLine(String.Format("1Script Execution Engine. Version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            Output.WriteLine();
            Output.WriteLine("Usage:");
            Output.WriteLine();
            Output.WriteLine("I. Script execution: oscript.exe <script_path> [script arguments..]");
            Output.WriteLine();
            Output.WriteLine("II. Special mode: oscript.exe <mode> <script_path> [script arguments..]");
            Output.WriteLine("Mode can be one of these:");
            Output.WriteLine(String.Format("  {0,-12}measures execution time", "-measure"));
            Output.WriteLine(String.Format("  {0,-12}shows compiled module without execution", "-compile"));
            Output.WriteLine(String.Format("  {0,-12}provides syntax check", "-check"));
            Output.WriteLine(String.Format("  {0,-12}provides syntax check in CGI-mode", "-check -cgi"));
            Output.WriteLine();
            Output.WriteLine(String.Format("  {0} set output encoding", "-encoding=<encoding-name>"));
            Output.WriteLine(String.Format("  {0} write code statistics", "-codestat=<filename>"));
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
