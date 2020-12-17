/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine.HostedScript
{
    public class FormatStringConfigProvider : IConfigProvider
    {
        public string ValuesString { get; set; }
        
        public Func<IDictionary<string, string>> GetProvider()
        {
            var localCopy = ValuesString;
            return () =>
            {
                var paramList = new FormatParametersList(localCopy);
                return paramList.ToDictionary();
            };
        }
    }
}