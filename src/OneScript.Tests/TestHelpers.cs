using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneScript.Tests
{
    static class TestHelpers
    {
        public static bool ExceptionThrown(Action action, Type exceptionType)
        {
            try
            {
                action();
                return false;
            }
            catch (Exception e)
            {
                if (e.GetType() == exceptionType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    class ExpectedExceptionMsgAttribute : ExpectedExceptionBaseAttribute
    {
        Type _excType;

        public ExpectedExceptionMsgAttribute(Type exceptionType)
        {
            _excType = exceptionType;
        }

        public string ExpectedMessage { get; set; }

        protected override void Verify(Exception exception)
        {
            base.RethrowIfAssertException(exception);

            Assert.IsInstanceOfType(exception, _excType);
            Assert.IsTrue(exception.Message.Contains(ExpectedMessage), String.Format("Could not verify the exception message {0}", exception.Message));
        }
    }
}
