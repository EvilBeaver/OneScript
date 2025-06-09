/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScript.Contexts
{
    /// <summary>
    /// Помечает статический метод класса, как Bsl-конструктор.
    /// Должен располагаться на статических методах.
    /// Поддерживает маршаллинг аргументов, т.е. можно указывать не только IValue, но и просто типы C#
    /// Первым параметром метода может быть указан TypeActivationContext, тогда он будет инжектиться в вызов.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ScriptConstructorAttribute : Attribute
    {
        public string Name { get; set; }
    }
}