/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary;

namespace BslTestsBridge.BslBridge
{
    public sealed class BslLogMessage
    {
        public BslLogMessage(string text, MessageStatusEnum status)
        {
            Text = text ?? string.Empty;
            Status = status;
        }

        public string Text { get; }

        public MessageStatusEnum Status { get; }

        public override string ToString()
        {
            return $"{Status}: {Text}";
        }
    }
}

