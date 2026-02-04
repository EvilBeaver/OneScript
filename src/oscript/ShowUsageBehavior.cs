/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace oscript
{
	internal class ShowUsageBehavior : AppBehavior
	{
		public override int Execute()
		{
            Output.WriteLine($"1Script Execution Engine. Version {Program.GetVersion()}");
            Output.WriteLine();
            Output.WriteLine("Usage:");
            Output.WriteLine("  oscript.exe [options] <script_path> [script_arguments...]");
            Output.WriteLine("  oscript.exe <mode> [mode_options] <script_path> [script_arguments...]");
            Output.WriteLine();

            const int modeWidth = -18;
            const int subOptionWidth = -14;

            Output.WriteLine("Modes:");
            Output.WriteLine($"  {"-measure",modeWidth} Measures script execution time.");
            Output.WriteLine($"  {"-compile",modeWidth} Shows compiled module without execution.");
            Output.WriteLine($"  {"-check",modeWidth} Provides syntax check.");
            Output.WriteLine($"  {"",modeWidth}   Options:");
            Output.WriteLine($"  {"",modeWidth}     {"-cgi",subOptionWidth} Syntax check in CGI-mode.");
            Output.WriteLine($"  {"",modeWidth}     {"-env=<file>",subOptionWidth} Path to entrypoint file for context.");
            
            Output.WriteLine($"  {"-debug",modeWidth} Runs script in debug mode.");
            Output.WriteLine($"  {"",modeWidth}   Options:");
            Output.WriteLine($"  {"",modeWidth}     {"-port=<port>",subOptionWidth} Debugger port (default is 2801).");
            Output.WriteLine($"  {"",modeWidth}     {"-noWait",subOptionWidth} Do not wait for debugger connection.");
            
            Output.WriteLine($"  {"-version, -v",modeWidth} Output version string.");
            Output.WriteLine();
            
            Output.WriteLine("Options:");
            Output.WriteLine($"  {"-encoding=<name>",modeWidth} Set output encoding (e.g. utf-8).");
            Output.WriteLine($"  {"-codestat=<file>",modeWidth} Write code execution statistics to file.");
            Output.WriteLine();
            
            Output.WriteLine("CGI Mode:");
            Output.WriteLine("  oscript.exe -cgi <script_path> [script_arguments...]");
            Output.WriteLine("  Runs as CGI application under HTTP-server.");

			return 0;
		}
	}
}