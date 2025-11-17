/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Специализированный TestFixture для выполнения BSL-тестов.
    /// Позволяет вручную подставлять методы жизненного цикла и добавлять тестовые методы.
    /// </summary>
    internal class BslTestFixture : TestFixture
    {
        public BslTestFixture(ITypeInfo fixtureType, object[] arguments = null)
            : base(fixtureType, arguments)
        {
        }

        public void ApplyLifecycleMethods(
            IMethodInfo[] beforeAll,
            IMethodInfo[] beforeEach,
            IMethodInfo[] afterEach,
            IMethodInfo[] afterAll)
        {
            OneTimeSetUpMethods = beforeAll ?? Array.Empty<IMethodInfo>();
            SetUpMethods = beforeEach ?? Array.Empty<IMethodInfo>();
            TearDownMethods = afterEach ?? Array.Empty<IMethodInfo>();
            OneTimeTearDownMethods = afterAll ?? Array.Empty<IMethodInfo>();
        }

        public TestMethod AddBslTest(
            IMethodInfo methodInfo,
            string displayName,
            string fullName,
            BslTestDescriptionDto description)
        {
            if (methodInfo == null)
                throw new ArgumentNullException(nameof(methodInfo));

            var testMethod = new TestMethod(methodInfo, this);

            if (!string.IsNullOrEmpty(displayName))
                testMethod.Name = displayName;

            if (!string.IsNullOrEmpty(fullName))
                testMethod.FullName = fullName;

            if (description != null)
                testMethod.Properties.Set("BslTestDescription", description);

            Add(testMethod);

            return testMethod;
        }
    }
}

