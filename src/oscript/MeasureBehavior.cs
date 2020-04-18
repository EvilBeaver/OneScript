/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace oscript
{
	internal class MeasureBehavior : ExecuteScriptBehavior
	{
		public MeasureBehavior(string path, string[] args) : base(path, args)
		{
		}

		public override int Execute()
		{
			var sw = new Stopwatch();
			Output.WriteLine("Script started: " + DateTime.Now + "\n");
			sw.Start();
			var exitCode = base.Execute();
			sw.Stop();
			Output.WriteLine("\nScript completed: " + DateTime.Now);
			Output.WriteLine("\nDuration: " + sw.Elapsed);
			return exitCode;
		}

		public static AppBehavior Create(CmdLineHelper helper)
		{
			var path = helper.Next();
            return path != null ? new MeasureBehavior(path, helper.Tail()) : null;
		}
	}
}