/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Commons;

namespace OneScript.Contexts
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ContextPropertyAttribute : Attribute, INameAndAliasProvider
    {
        private readonly string _name;
        private readonly string _alias;
        private Type _converter;

        public ContextPropertyAttribute(string name, string alias = "")
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            _name = name;
            _alias = alias;
            CanRead = true;
            CanWrite = true;
        }

        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        /// <summary>
        /// Данное свойство не будет обработано генератором документации при обходе типов
        /// </summary>
        public bool SkipForDocumenter { get; set; }

        public string Name => _name;
        public string Alias => _alias;

        /// <summary>
        /// Конвертер значения свойства
        /// </summary>
        public Type Converter
        {
            get => _converter;
            set
            {
                if (value != null)
                {
                    if (!value.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IContextValueConverter<>)))
                        throw new Exception("Конвертер должен реализовывать интерфейс IContextValueConverter");

                    if (value.IsAbstract || value.IsInterface)
                        throw new Exception("Конвертер не может быть абстрактным типом или интерфейсом");
                }
                
                _converter = value;
            }
        }
    }
}