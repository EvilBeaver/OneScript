/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Types
{
    /// <summary>
    /// Разрешает тип, отсутствующий в реестре, в момент первого обращения к нему.
    /// Реализации регистрируются в контейнере движка через
    /// IServiceDefinitions.RegisterEnumerable и передаются DefaultTypeManager
    /// через конструктор.
    /// </summary>
    /// <remarks>
    /// Реализация должна быть дешёвой: без загрузки библиотек и вызовов нативного кода.
    /// Метод обязан быть идемпотентным: повторный вызов с тем же именем возвращает тот же дескриптор.
    /// Для нераспознанного имени возвращается false, исключения не бросаются.
    /// Найденный тип регистрируется через RegisterType(string, string, Type), а не RegisterType(TypeDescriptor).
    /// </remarks>
    public interface ILazyTypeResolver
    {
        /// <summary>
        /// Пытается разрешить тип по имени.
        /// </summary>
        /// <param name="typeName">Имя типа, запрошенное из скрипта.</param>
        /// <param name="typeManager">
        /// Менеджер, запросивший разрешение; именно в него регистрируется
        /// найденный тип. Передаётся параметром, а не внедряется в конструктор,
        /// чтобы не создавать цикл зависимостей в DI.
        /// </param>
        /// <param name="type">Найденный дескриптор типа, если метод вернул true.</param>
        /// <returns>true, если тип распознан и зарегистрирован в typeManager</returns>
        bool TryResolve(string typeName, ITypeManager typeManager, out TypeDescriptor type);
    }
}
