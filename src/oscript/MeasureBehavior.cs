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
    class MeasureBehavior : ExecuteScriptBehavior
    {
        public MeasureBehavior(string path, string[] args) : base(path,args)
        {

        }

        public override int Execute()
        {
            var sw = new System.Diagnostics.Stopwatch();
            Console.WriteLine("Script started: " + DateTime.Now.ToString() + "\n");
            sw.Start();
            int exitCode = base.Execute();
            sw.Stop();
            Console.WriteLine("\nScript completed: " + DateTime.Now.ToString());
            Console.WriteLine("\nDuration: " + sw.Elapsed.ToString());
            return exitCode;
        }
    }
}
