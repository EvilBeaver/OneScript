/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using Moq;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class MethodDispatcherTest
    {
        [Fact]
        public void TestVoidCallOfInterface()
        {
            var instance = new Mock<IDebuggerService>();
            instance.Setup(i => i.Execute(It.IsAny<int>()));
            
            var dispatcher = new MethodsDispatcher<IDebuggerService>();
            dispatcher.Dispatch(instance.Object, "Execute", new object[] { 1 });
            instance.Verify(i => i.Execute(1), Times.Once);
            instance.VerifyNoOtherCalls();
        }
        
        [Fact]
        public void TestFunctionCallOfInterface()
        {
            var instance = new Mock<IDebuggerService>();
            instance.Setup(i => i.GetThreads()).Returns(new []{1,2,3});

            var dispatcher = new MethodsDispatcher<IDebuggerService>();
            var result = (int[])dispatcher.Dispatch(instance.Object, "GetThreads", new object[0]);
            instance.Verify(i => i.GetThreads(), Times.Once);
            instance.VerifyNoOtherCalls();
            result.Should().HaveCount(3);
        }
    }
}