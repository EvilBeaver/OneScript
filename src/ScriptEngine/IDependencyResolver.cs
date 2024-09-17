/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Compilation;

namespace ScriptEngine
{
    /// <summary>
    /// Разрешитель внешних зависимостей (библиотек)
    /// </summary>
    public interface IDependencyResolver : ICompileTimeDependencyResolver
    {
        /// <summary>
        /// Инициализировать разрешитель. Вызывается при создании ScriptingEngine
        /// </summary>
        /// <param name="engine">Движок, который разрешитель может сохранить у себя</param>
        void Initialize(ScriptingEngine engine);
    }
}