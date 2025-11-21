/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OneScript.StandardLibrary;
using OneScript.Values;

namespace BslTestsBridge.BslBridge
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

        public void FlushIntoWriter(TextWriter writer)
        {
            foreach (var message in Messages)
            {
                writer.WriteLine(message.Text);
            }
        }
        
        public void FlushIntoWriter(TextWriter infoWriter, TextWriter errorWriter)
        {
            foreach (var message in Messages)
            {
                if (message.Status == MessageStatusEnum.Ordinary ||
                    message.Status == MessageStatusEnum.WithoutStatus ||
                    message.Status == MessageStatusEnum.Information)
                {
                    infoWriter.WriteLine(message.Text);
                }
                else
                {
                    errorWriter.WriteLine(message.Text);
                }
            }
        }
    }
}

