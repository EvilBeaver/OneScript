/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;

namespace OneScript.DebugProtocol.TcpServer
{
    public class StreamUtils
    {
        public static void ReadStream(Stream stream, byte[] buffer, int length)
        {
            int readPosition = 0;
            int bytesReceived = 0;

            while (bytesReceived < length)
            {
                var portionSize = stream.Read(buffer, readPosition, length - bytesReceived);
                if (portionSize == 0)
                    throw new IOException("Unexpected end of stream");
                
                readPosition += portionSize;
                bytesReceived += portionSize;
            }
        }
    }
}