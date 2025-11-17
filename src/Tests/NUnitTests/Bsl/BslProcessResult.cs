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
using OneScript.Values;

namespace NUnitTests.Bsl
{
    public sealed class BslProcessResult
    {
        public BslProcessResult(BslValue methodResult, IReadOnlyList<BslLogMessage> messages)
        {
            MethodResult = methodResult;
            Messages = messages ?? Array.Empty<BslLogMessage>();
        }

        public BslValue MethodResult { get; }
        
        public IReadOnlyList<BslLogMessage> Messages { get; }

        public string GetCombinedLog()
        {
            var builder = new StringBuilder();
            foreach (var message in Messages)
            {
                builder.AppendLine(message.Text);
            }

            return builder.ToString();
        
        }
    }
}

