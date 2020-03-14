/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
namespace OneScript.DebugProtocol
{
    public static class DebuggerSettings
    {
        public const int MAX_BUFFER_SIZE = 5000000;
        public const int MAX_PRESENTATION_LENGTH = (int)(MAX_BUFFER_SIZE / 2.5);
    }
}