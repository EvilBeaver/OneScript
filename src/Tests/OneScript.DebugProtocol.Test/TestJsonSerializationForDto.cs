/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Newtonsoft.Json;
using OneScript.DebugProtocol.TcpServer;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class TestJsonSerializationForDto
    {
        [Theory]
        [MemberData(nameof(DebuggerServiceCalls))]
        public void CanSerializeAllMethodCallsForServer(RpcCall call, RpcCallResult? result)
        {
            // Не должен падать
            var jsonCall = JsonConvert.SerializeObject(call);
            string jsonResult = null;
            if (result != null)
            {
                jsonResult = JsonConvert.SerializeObject(result);
            }
            
            // должен суметь прочитать
            var callFromJson = JsonConvert.DeserializeObject<RpcCall>(jsonCall);
            callFromJson.Should().BeEquivalentTo(call);
            if (jsonResult != null)
            {
                var resultFromJson = JsonConvert.DeserializeObject<RpcCallResult>(jsonResult);
                resultFromJson.Should().BeEquivalentTo(result);
            }
        }

        [Fact]
        public void CanReadWriteCallWithNullParams()
        {
            var call = RpcCall.Create("Test", "Test", null);
            var jsonCall = JsonConvert.SerializeObject(call);
            var callFromJson = JsonConvert.DeserializeObject<RpcCall>(jsonCall);
            
            callFromJson.Should().BeEquivalentTo(call);
        }
        
        [Fact]
        public void CanReadWriteResultWithNullValue()
        {
            var call = RpcCall.Create("Test", "Test", null);
            var result = RpcCallResult.Respond(call, null);
            var jsonResult = JsonConvert.SerializeObject(result);
            var resultFromJson = JsonConvert.DeserializeObject<RpcCallResult>(jsonResult);
            
            resultFromJson.Should().BeEquivalentTo(result);
        }

        public static IEnumerable<object[]> DebuggerServiceCalls()
        {
            var allMethods = typeof(IDebuggerService).GetMethods()
                .Where(mi => mi.GetCustomAttribute<ObsoleteAttribute>() == null);
            
            var fixture = new Fixture();
            foreach (var methodInfo in allMethods)
            {
                var parameters = methodInfo.GetParameters()
                    .Select(p => new SpecimenContext(fixture).Resolve(p.ParameterType))
                    .ToArray();
                
                var call = RpcCall.Create(nameof(IDebuggerService), methodInfo.Name, parameters);
                var callResult = methodInfo.ReturnType == typeof(void)?
                    null:
                    RpcCallResult.Respond(call, new SpecimenContext(fixture).Resolve(methodInfo.ReturnType));
                
                yield return new object[] { call, callResult };
            }
        }
    }
}