/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    /// <summary>
    /// Описание нажатия клавиши, полученное методом <see cref="ConsoleContext.ReadKeyPress"/>.
    /// Содержит код клавиши, введённый символ и состояние модификаторов.
    /// </summary>
    /// <remarks>
    /// Свойства объекта доступны только для чтения и имеют два имени — русское и английское.
    /// Как и любой результат чтения консоли, объект нельзя создать оператором <c>Новый</c>:
    /// он возвращается только методом <see cref="ConsoleContext.ReadKeyPress"/>.
    /// </remarks>
    [ContextClass("НажатиеКлавиши", "KeyPress")]
    public class ConsoleKeyPress : AutoContext<ConsoleKeyPress>
    {
        private readonly ConsoleKeyInfo _keyInfo;

        internal ConsoleKeyPress(ConsoleKeyInfo keyInfo)
        {
            _keyInfo = keyInfo;
        }

        /// <summary>
        /// Код нажатой клавиши. Совпадает со значением, которое возвращает <see cref="ConsoleContext.ReadKey"/>.
        /// </summary>
        [ContextProperty("КодКнопки", "KeyCode", CanWrite = false)]
        public int KeyCode => (int)_keyInfo.Key;

        /// <summary>
        /// Введённый символ, в том числе кириллический.
        /// Для служебных клавиш и управляющих сочетаний содержит пустую строку.
        /// </summary>
        [ContextProperty("Символ", "Symbol", CanWrite = false)]
        public string Symbol => char.IsControl(_keyInfo.KeyChar) ? string.Empty : _keyInfo.KeyChar.ToString();

        /// <summary>
        /// Признак того, что при нажатии была удержана клавиша Shift.
        /// </summary>
        [ContextProperty("Shift", CanWrite = false)]
        public bool Shift => (_keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;

        /// <summary>
        /// Признак того, что при нажатии была удержана клавиша Alt.
        /// </summary>
        [ContextProperty("Alt", CanWrite = false)]
        public bool Alt => (_keyInfo.Modifiers & ConsoleModifiers.Alt) != 0;

        /// <summary>
        /// Признак того, что при нажатии была удержана клавиша Control (Ctrl).
        /// </summary>
        [ContextProperty("Control", CanWrite = false)]
        public bool Control => (_keyInfo.Modifiers & ConsoleModifiers.Control) != 0;

        public override string ToString()
        {
            var keyPresentation = Symbol.Length != 0 ? Symbol : _keyInfo.Key.ToString();
            var modifiersPresentation = GetModifiersPresentation();

            return modifiersPresentation.Length != 0
                ? string.Concat(modifiersPresentation, "+", keyPresentation)
                : keyPresentation;
        }

        private string GetModifiersPresentation()
        {
            var modifiers = new List<string>(3);

            if (Shift)
                modifiers.Add("Shift");
            if (Alt)
                modifiers.Add("Alt");
            if (Control)
                modifiers.Add("Control");

            return string.Join("+", modifiers);
        }
    }
}
