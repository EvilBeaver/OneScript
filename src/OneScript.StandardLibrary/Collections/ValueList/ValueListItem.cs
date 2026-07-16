/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Localization;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueList
{
    /// <summary>
    /// Используется для доступа к свойствам и методам элемента списка значений
    /// </summary>
    [ContextClass("ЭлементСпискаЗначений", "ValueListItem")]
    public class ValueListItem : AutoContext<ValueListItem>
    {
        static readonly BilingualString EMPTY_VALUE_STRING = new("<Пустое значение>", "<Empty value>");

        private string _presentationHolder;
        private IValue _pictureHolder;

        internal ValueListItem()
        {
            _pictureHolder = ValueFactory.Create();
            _presentationHolder = String.Empty;
        }
        
        [ContextProperty("Значение", "Value")]
        public IValue Value { get; set; }

        [ContextProperty("Представление", "Presentation")]
        public string Presentation
        {
            get => _presentationHolder;
            set => _presentationHolder = value ?? String.Empty;
        }

        [ContextProperty("Пометка", "Check")]
        public bool Check { get; set; }

        [ContextProperty("Картинка", "Picture")]
        public IValue Picture
        {
            get => _pictureHolder;
            set => _pictureHolder = value ?? ValueFactory.Create();
        }

        public override string ToString()
            => !String.IsNullOrEmpty(_presentationHolder) ? _presentationHolder
            : Value is BslUndefinedValue ? EMPTY_VALUE_STRING
            : Value.ToString();
    }
}
