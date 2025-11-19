/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using OneScript.Contexts;

namespace NUnitTests.Bsl
{
    internal class TestCaseMethodDecorator : IScriptMethodDecorator<InvokableBslMethodInfo>
    {
        private readonly BslScriptProcess _bslExecutor;
        private readonly LifecycleMethods _lifecycleMethods;
        private readonly IReadOnlyCollection<string> _testMethods;

        public TestCaseMethodDecorator(BslScriptProcess bslExecutor, LifecycleMethods lifecycleMethods, IReadOnlyCollection<string> testMethods)
        {
            _bslExecutor = bslExecutor;
            _lifecycleMethods = lifecycleMethods;
            _testMethods = testMethods;
        }

        public InvokableBslMethodInfo Convert(BslScriptMethodInfo originalMethod)
        {
            return new InvokableBslMethodInfo(originalMethod, _bslExecutor);
        }

        public void BuildUp(BslScriptMethodInfo originalMethod, BslMethodBuilder<InvokableBslMethodInfo> builder)
        {
            var methodName = originalMethod.Name;
                    
            // Проверяем, является ли метод методом жизненного цикла и добавляем соответствующую аннотацию NUnit
            if (_lifecycleMethods.BeforeAll.Contains(methodName, StringComparer.OrdinalIgnoreCase))
            {
                builder.SetAnnotations(new[] { new OneTimeSetUpAttribute() });
            }
            else if (_lifecycleMethods.BeforeEach.Contains(methodName, StringComparer.OrdinalIgnoreCase))
            {
                builder.SetAnnotations(new[] { new SetUpAttribute() });
            }
            else if (_lifecycleMethods.AfterEach.Contains(methodName, StringComparer.OrdinalIgnoreCase))
            {
                builder.SetAnnotations(new[] { new TearDownAttribute() });
            }
            else if (_lifecycleMethods.AfterAll.Contains(methodName, StringComparer.OrdinalIgnoreCase))
            {
                builder.SetAnnotations(new[] { new OneTimeTearDownAttribute() });
            }
            // Проверяем, является ли метод тестовым методом и добавляем TestAttribute
            else if (_testMethods.Contains(methodName))
            {
                builder.SetAnnotations(new[] { new TestAttribute() });
            }
        }
    }
}