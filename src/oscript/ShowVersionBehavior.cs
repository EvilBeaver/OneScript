/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;

namespace oscript
{
    class ShowVersionBehavior : AppBehavior
    {
        public override int Execute()
        {
            Output.WriteLine(String.Format("{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version));
            return 0;
        }
    }
}