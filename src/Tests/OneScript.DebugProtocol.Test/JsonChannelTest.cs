/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.IO;
using FluentAssertions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.Exceptions;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class JsonChannelTest
    {
        
        [Fact]
        public void TestObjectWritingReadingCall()
        {
            var dataStream = new MemoryStream();
            var rpcCall = RpcCall.Create("Test", "Hello World", 1, 2, new Breakpoint()
            {
                Line = 2, Condition = "true"
            });
            
            var channel = new JsonDtoChannel(dataStream);
            channel.Write(rpcCall);
            dataStream.Position = 0;
            var actual = channel.Read<RpcCall>();
            actual.Should().BeEquivalentTo(rpcCall);
        }
        
        [Fact]
        public void TestObjectWritingReadingResponseSuccess()
        {
            var dataStream = new MemoryStream();
            var rpcCall = RpcCall.Create("Test", "Hello World", 1, 2, new Breakpoint()
            {
                Line = 2, Condition = "true"
            });

            var rpcResponse = RpcCallResult.Respond(rpcCall, new Variable
            {
                Name = "Test",
                Presentation = "1"
            });
            
            var channel = new JsonDtoChannel(dataStream);
            channel.Write(rpcResponse);
            dataStream.Position = 0;
            var actual = channel.Read<RpcCallResult>();
            actual.Should().BeEquivalentTo(rpcResponse);
        }
        
        [Fact]
        public void TestObjectWritingReadingResponseException()
        {
            var dataStream = new MemoryStream();
            var rpcCall = RpcCall.Create("Test", "Hello World", 1, 2, new Breakpoint()
            {
                Line = 2, Condition = "true"
            });

            var rpcResponse = RpcCallResult.Exception(rpcCall, new RuntimeException("Test", "Test"));
            
            var channel = new JsonDtoChannel(dataStream);
            channel.Write(rpcResponse);
            dataStream.Position = 0;
            var actual = channel.Read<RpcCallResult>();
            actual.Should().BeEquivalentTo(rpcResponse);
        }

        [Fact]
        public void TestReconcileMarker()
        {
            var encoded = FormatReconcileUtils.EncodeFormatMarker(2, 4);
            var (transport, version) = FormatReconcileUtils.DecodeFormatMarker(encoded);
            
            Assert.Equal(2, transport);
            Assert.Equal(4, version);
        }
    }
}
