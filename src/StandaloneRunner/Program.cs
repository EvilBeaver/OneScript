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

namespace StandaloneRunner
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			return Run(args);
		}

		private static int Run(string[] args)
		{
			var sp = new StandaloneProcess
			{
				CommandLineArguments = args
			};
			return sp.Run();
		}
	}
}