/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    /// <summary>
    /// Список расширенных имен XML.
    /// </summary>
    /// <see cref="XMLExpandedName"/>
    /// <seealso cref="XMLSchema.Objects.XSSimpleTypeDefinition"/>
    public class XMLExpandedNameList: AutoCollectionContext<XMLExpandedNameList, XMLExpandedName>
    {
        private readonly List<XMLExpandedName> _items;

        private XMLExpandedNameList() => _items = new List<XMLExpandedName>();

        /// <summary>
        /// Создает новый список расширенных имен XML.
        /// </summary>
        /// <returns>Список расширенных имен XML.</returns>
        public static XMLExpandedNameList Create() => new XMLExpandedNameList();

        #region OneScript

        #region Methods

        /// <summary>
        /// Вставляет расширенное имя в коллекцию по указанному индексу.
        /// </summary>
        /// <param name="index">Отсчитываемый от нуля индекс, по которому следует вставить элемент value.</param>
        /// <param name="value">Расширенное имя XML</param>
        [ContextMethod("Вставить", "Insert")]
        public void Insert(int index, XMLExpandedName value) => _items.Insert(index, value);

        /// <summary>
        /// Добавляет расширенное имя в коллекцию.
        /// </summary>
        /// <param name="value">Расширенное имя XML</param>
        [ContextMethod("Добавить", "Add")]
        public void Add(XMLExpandedName value) => _items.Add(value);
        
        /// <summary>
        /// Получает количество элементов коллекции.
        /// </summary>
        /// <returns>Количество элементов</returns>
        [ContextMethod("Количество", "Count")]
        public override int Count() => _items.Count;

        /// <summary>
        /// Очищает коллекцию
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear() => _items.Clear();

        /// <summary>
        /// Возвращает расширенное имя XML по индексу.
        /// </summary>
        /// <param name="index">Индекс значения в списке</param>
        /// <returns>Расширенное имя XML</returns>
        [ContextMethod("Получить", "Get")]
        public XMLExpandedName Get(int index) => _items[index];

        /// <summary>
        /// Удаляет элемент с указанным индексом из списка.
        /// </summary>
        /// <param name="index">Индекс</param>
        [ContextMethod("Удалить", "Delete")]
        public void Delete(int index) => _items.RemoveAt(index);

        /// <summary>
        /// Устанавливает расширенное имя XML в коллекцию по указанному индексу.
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <param name="value">Расширенное имя XML</param>
        [ContextMethod("Установить", "Set")]
        public void Set(int index, XMLExpandedName value) => _items[index] = value;
               
        #endregion

        #endregion

        #region IEnumerable

        public override IEnumerator<XMLExpandedName> GetEnumerator() => _items.GetEnumerator();

        #endregion
    }
}


