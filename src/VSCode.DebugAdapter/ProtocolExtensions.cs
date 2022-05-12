/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using VSCodeDebug;
using StackFrame = OneScript.DebugProtocol.StackFrame;

namespace VSCode.DebugAdapter
{
    public static class ProtocolExtensions
    {
        private static readonly char[] specialChars = new char[] { '<', '>' };
        
        public static bool IsStringModule(this StackFrame frame)
        {
            return frame.Source.IndexOfAny(specialChars) != -1;
        }

        public static Source GetSource(this StackFrame frame)
        {
            if (frame.IsStringModule())
            {
                return new Source(frame.Source, null)
                {
                    origin = frame.Source,
                    presentationHint = "deemphasize"
                };
            }
            
            return new Source(frame.Source);
        }
    }
}