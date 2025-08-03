/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Text;
using OneScript.DebugProtocol.Internal;

namespace OneScript.DebugProtocol.TcpServer
{
    public static class FormatReconcileUtils
    {
        /// <summary>
        /// Специальный пакет, который будет принят двоичным десериализатором старого движка, но который потом
        /// упадет на приведении типов.
        /// В новом движке он будет принят, как команда для обмена версией формата.
        /// </summary>
        public static byte[] GetReconcileMagic()
        {
            using (var memoryStream = new MemoryStream(256))
            {
                using (var writer = new BinaryFormatWriter(memoryStream))
                {
                    writer.WriteHeader(1, 1);
                    writer.WriteLibrary(typeof(RpcCall).Assembly, 1);
                    writer.WriteClassWithNoFields(typeof(RpcCall), 1, 1);
                    //writer.WriteInstance(new object[]{Array.Empty<object>(), "$NonExistentMethod$", nameof(IDebuggerService)});
                    writer.WriteEnd();
                }

                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Ответ на запрос формата
        /// </summary>
        public static readonly byte[] FORMAT_RECONCILE_RESPONSE_PREFIX =
        {
            0x1C,
            0x1C,
            0x1C,
            0x1C
        };
        
        public static readonly TimeSpan FORMAT_RECONCILE_TIMEOUT = TimeSpan.FromMilliseconds(500);

        public static bool CheckReconcilePrefix(byte[] data)
        {
            for (int i = 0; i < FORMAT_RECONCILE_RESPONSE_PREFIX.Length; i++)
            {
                if (data[i] != FORMAT_RECONCILE_RESPONSE_PREFIX[i]) 
                    return false;
            }

            return true;
        }

        public static bool CheckReconcileRequest(Stream stream)
        {
            var magic = GetReconcileMagic();
            var buffer = new byte[magic.Length];
            ReadStream(stream, buffer, buffer.Length);
            for (int i = 0; i < magic.Length; i++)
            {
                if (buffer[i] != magic[i]) 
                    return false;
            }

            return true;
        }

        public static void WriteReconcileResponse(Stream target, short transport, short version)
        {
            var formatInfo = EncodeFormatMarker(transport, version);
                
            using var binaryWriter = new BinaryWriter(target, Encoding.UTF8, true);
            binaryWriter.Write(FORMAT_RECONCILE_RESPONSE_PREFIX);
            binaryWriter.Write(formatInfo);
        }

        public static int EncodeFormatMarker(short transport, short dataVersion)
        {
            var marker = transport << 16;
            return marker | dataVersion;
        }
        
        public static (int, int) DecodeFormatMarker(int marker)
        {
            var transport = marker >> 16;
            var dataVersion = marker & 0x0000FFFF;

            return (transport, dataVersion);
        }
        
        private static void ReadStream(Stream stream, byte[] buffer, int length)
        {
            int readPosition = 0;
            int bytesReceived = 0;

            while (bytesReceived < length)
            {
                bytesReceived = stream.Read(buffer, readPosition, length - bytesReceived);
                if (bytesReceived == 0)
                    throw new IOException("Unexpected end of stream");
                
                readPosition += bytesReceived;
            }
        }
    }
}
