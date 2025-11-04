using System;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class DeprecationWarningScopeTests
    {
        [Fact]
        public void Suppression_Nested_Increments_And_Restores()
        {
            Assert.False(DeprecationWarningScope.IsSuppressed);
            using (DeprecationWarningScope.Suppress())
            {
                Assert.True(DeprecationWarningScope.IsSuppressed);
                using (DeprecationWarningScope.Suppress())
                {
                    Assert.True(DeprecationWarningScope.IsSuppressed);
                }
                Assert.True(DeprecationWarningScope.IsSuppressed);
            }
            Assert.False(DeprecationWarningScope.IsSuppressed);
        }

        [Fact]
        public void Suppression_Dispose_Is_Idempotent()
        {
            var token = DeprecationWarningScope.Suppress();
            Assert.True(DeprecationWarningScope.IsSuppressed);
            token.Dispose();
            Assert.False(DeprecationWarningScope.IsSuppressed);
            token.Dispose();
            Assert.False(DeprecationWarningScope.IsSuppressed);
        }
    }
}