/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using Moq;
using NUnit.Framework;
using OneScript.DebugProtocol;

namespace NUnitTests
{
    [TestFixture]
    public class MethodDispatcherTest
    {
        [Test]
        public void TestVoidCallOfInterface()
        {
            var instance = new Mock<IDebuggerService>();
            instance.Setup(i => i.Execute(It.IsAny<int>()));
            
            var dispatcher = new MethodsDispatcher<IDebuggerService>();
            dispatcher.Dispatch(instance.Object, "Execute", new object[] { 1 });
            instance.Verify(i => i.Execute(1), Times.Once);
            instance.VerifyNoOtherCalls();
        }
        
        [Test]
        public void TestFunctionCallOfInterface()
        {
            var instance = new Mock<IDebuggerService>();
            instance.Setup(i => i.GetThreads()).Returns(new []{1,2,3});

            var dispatcher = new MethodsDispatcher<IDebuggerService>();
            var result = dispatcher.Dispatch(instance.Object, "GetThreads", new object[0]);
            instance.Verify(i => i.GetThreads(), Times.Once);
            instance.VerifyNoOtherCalls();
            Assert.That(result, Has.Length.EqualTo(3));
        }
    }
}