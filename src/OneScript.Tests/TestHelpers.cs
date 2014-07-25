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
}
