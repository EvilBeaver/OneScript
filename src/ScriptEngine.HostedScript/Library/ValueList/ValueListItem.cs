/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library.ValueList
{
    /// <summary>
    /// Используется для доступа к свойствам и методам элемента списка значений
    /// </summary>
    [ContextClass("ЭлементСпискаЗначений", "ValueListItem")]
    public class ValueListItem : AutoContext<ValueListItem>
    {
        private string _presentationHolder;
        private IValue _pictureHolder;

        public ValueListItem()
        {
            _pictureHolder = ValueFactory.Create();
            _presentationHolder = String.Empty;
        }
        
        [ContextProperty("Значение", "Value")]
        public IValue Value
        {
            get;
            set;
        }

        [ContextProperty("Представление", "Presentation")]
        public string Presentation
        {
            get
            {
                return _presentationHolder;
            }
            set
            {
                if (value == null)
                    _presentationHolder = String.Empty;
                else
                    _presentationHolder = value;
            }
        }

        [ContextProperty("Пометка", "Check")]
        public bool Check
        {
            get;
            set;
        }

        [ContextProperty("Картинка", "Picture")]
        public IValue Picture
        {
            get
            {
                return _pictureHolder;
            }
            set
            {
                if(value != null)
                {
                    _pictureHolder = value;
                }
                else
                {
                    _pictureHolder = ValueFactory.Create();
                }
            }
        }
    }
}
