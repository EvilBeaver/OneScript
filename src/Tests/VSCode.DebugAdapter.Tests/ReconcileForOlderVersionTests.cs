/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FluentAssertions;
using OneScript.DebugProtocol;
using OneScript.DebugProtocol.TcpServer;
using Xunit;

namespace VSCode.DebugAdapter.Tests
{
    public class ReconcileForOlderVersionTests
    {
        [Fact]
        public void CanReadReconcileViaBinaryFormatter()
        {
            using (var stream = new MemoryStream(FormatReconcileUtils.GetReconcileMagic()))
            {
                var reader = new BinaryFormatter();
                
                var value = reader.Deserialize(stream);
                value.Should().BeEquivalentTo(new RpcCall());
            }
        }

        [Fact(Skip = "Manual run only")]
        public void DumpBinaryStream()
        {
            const string filePath = "<path>";
            using (var stream = new MemoryStream(FormatReconcileUtils.GetReconcileMagic()))
            {
                var serializer = new BinaryFormatter();
                var sampleCall = RpcCall.Create(nameof(IDebuggerService), "$NonExistent$");
                
                using (var dest = new MemoryStream())
                {
                    serializer.Serialize(dest, sampleCall);
                    using (var file = new FileStream(filePath, FileMode.Create))
                    {
                        dest.Position = 0;
                        dest.CopyTo(file);
                    }
                }
            }
        }
        
        [Fact]
        public void ExchangeReconcileVersions()
        {
            using (var stream = new MemoryStream(FormatReconcileUtils.GetReconcileMagic()))
            {
                using(var response = new MemoryStream())
                {
                    if (FormatReconcileUtils.CheckReconcileRequest(stream))
                    {
                        FormatReconcileUtils.WriteReconcileResponse(response, 1, 1);
                    }

                    var data = response.ToArray();
                    Assert.True(FormatReconcileUtils.CheckReconcilePrefix(data));
                    Assert.Equal(FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length + sizeof(int), data.Length);
                    
                    var frmt = BitConverter.ToInt32(data, FormatReconcileUtils.FORMAT_RECONCILE_RESPONSE_PREFIX.Length);
                    var (transport, version) = FormatReconcileUtils.DecodeFormatMarker(frmt);
                    Assert.Equal(1, transport);
                    Assert.Equal(1, version);
                }
            }
        }
    }
}
