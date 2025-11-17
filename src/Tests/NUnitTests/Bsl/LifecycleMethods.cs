/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace NUnitTests.Bsl
{
    /// <summary>
    /// Класс для хранения методов жизненного цикла теста.
    /// </summary>
    internal class LifecycleMethods
    {
        /// <summary>
        /// Методы, выполняемые один раз перед всеми тестами в фикстуре.
        /// </summary>
        public string[] BeforeAll { get; set; } = System.Array.Empty<string>();

        /// <summary>
        /// Методы, выполняемые перед каждым тестом.
        /// </summary>
        public string[] BeforeEach { get; set; } = System.Array.Empty<string>();

        /// <summary>
        /// Методы, выполняемые после каждого теста.
        /// </summary>
        public string[] AfterEach { get; set; } = System.Array.Empty<string>();

        /// <summary>
        /// Методы, выполняемые один раз после всех тестов в фикстуре.
        /// </summary>
        public string[] AfterAll { get; set; } = System.Array.Empty<string>();
    }
}

