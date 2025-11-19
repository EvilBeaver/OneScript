/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

#nullable enable

namespace OneScript.Contexts
{
    /// <summary>
    /// Интерфейс используется при построении CLR-типа на базе скриптового объекта в классе <see cref="ClassBuilder"/>
    /// Каждый оригинальный иммутабельный bsl-метод класса будет конвертирован даным декоратором
    /// и добавлен в CLR-тип, который строит <see cref="ClassBuilder"/>
    /// </summary>
    public interface IScriptMethodDecorator<T> where T : BslScriptMethodInfo
    {
        /// <summary>
        /// Первоначальное создание нового метода на базе оригинального.
        /// В принципе, может вернуть оригинальный метод, если он не требует конвертации.
        /// </summary>
        /// <returns>Должен вернуть инстанс метода, добавляемого в класс. Если вернет null, метод пропускается.</returns>
        T? Convert(BslScriptMethodInfo originalMethod);
        
        /// <summary>
        /// Модификация нового метода после его создания.
        /// Метод не вызывается, если Convert вернул оригинальный метод, т.к. bsl-метод иммутабельный.
        /// </summary>
        /// <param name="originalMethod">Оригинальный метод</param>
        /// <param name="builder">Билдер нового метода</param>
        void BuildUp(BslScriptMethodInfo originalMethod, BslMethodBuilder<T> builder);
    }
}