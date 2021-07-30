/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Collections.ValueList
{
    /// <summary>
    /// Стандартная универсальная коллекция системы 1С:Предприятие 8
    /// </summary>
    [ContextClass("СписокЗначений", "ValueList")]
    public class ValueListImpl : AutoCollectionContext<ValueListImpl, ValueListItem>
    {
        readonly List<ValueListItem> _items;
        public ValueListImpl()
        {
            _items = new List<ValueListItem>();
        }

        public override bool IsIndexed
        {
            get
            {
                return true;
            }
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.SystemType == BasicTypes.Number)
            {
                return GetValue(index);
            }
            else
            {
                return base.GetIndexedValue(index);
            }
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.SystemType == BasicTypes.Number)
            {
                throw new RuntimeException("Индексированное значение доступно только для чтения");
            }
            else
            {
                base.SetIndexedValue(index, val);
            }
        }

        /// <summary>
        /// Получить элемент по индексу
        /// </summary>
        /// <param name="index">Число - Индекс элемента</param>
        /// <returns>ЭлементСпискаЗначений</returns>
        [ContextMethod("Получить", "Get")]
        public ValueListItem GetValue(IValue index)
        {
            int numericIndex = (int)index.AsNumber();
            return _items[numericIndex];
        }

        /// <summary>
        /// Добавляет значение к списку
        /// </summary>
        /// <param name="value">Произвольный - Добавляемое значение</param>
        /// <param name="presentation">Строка (необязательный) - Строковое представление добавляемого значения</param>
        /// <param name="check">Булево (необязательный) - Определяет наличие пометки у добавляемого элемента</param>
        /// <param name="picture">Картинка (необязательный) - Визуальное  представление добавляемого значения</param>
        /// <returns>ЭлементСпискаЗначений</returns>
        [ContextMethod("Добавить", "Add")]
        public ValueListItem Add(IValue value, string presentation = null, bool check = false, IValue picture = null)
        {
            var newItem = CreateNewListItem(value, presentation, check, picture);

            _items.Add(newItem);
            return newItem;
        }

        /// <summary>
        /// Вставляет значение в список в указанную позицию
        /// </summary>
        /// <param name="index">Число - Индекс позиции, куда будет произведена вставка</param>
        /// <param name="value">Произвольный - Добавляемое значение</param>
        /// <param name="presentation">Строка (необязательный) - Строковое представление добавляемого значения</param>
        /// <param name="check">Булево (необязательный) - Определяет наличие пометки у добавляемого элемента</param>
        /// <param name="picture">Картинка (необязательный) - Визуальное  представление добавляемого значения</param>
        /// <returns>ЭлементСпискаЗначений</returns>
        [ContextMethod("Вставить", "Insert")]
        public ValueListItem Insert(int index, IValue value, string presentation = null, bool check = false, IValue picture = null)
        {
            var newItem = CreateNewListItem(value, presentation, check, picture);
            _items.Insert(index, newItem);
            
            return newItem;
        }

        private static ValueListItem CreateNewListItem(IValue value, string presentation, bool check, IValue picture)
        {
            var newItem = new ValueListItem();
            newItem.Value = value;
            newItem.Presentation = presentation;
            newItem.Check = check;
            newItem.Picture = picture;
            return newItem;
        }

        /// <summary>
        /// Выгружает значения в новый массив
        /// </summary>
        /// <returns>Массив</returns>
        [ContextMethod("ВыгрузитьЗначения", "UnloadValues")]
        public ArrayImpl UnloadValues()
        {
            return new ArrayImpl(_items.Select(x=>x.Value));
        }

        /// <summary>
        /// Загружает значения из массива
        /// </summary>
        /// <param name="source">Массив - Значения для загрузки в список</param>
        [ContextMethod("ЗагрузитьЗначения", "LoadValues")]
        public void LoadValues(ArrayImpl source)
        {
            Clear();
            _items.AddRange(source.Select(x => new ValueListItem() { Value = x }));
        }

        /// <summary>
        /// Удаляет все элементы из списка.
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Устанавливает значение пометки у всех элементов списка значений
        /// </summary>
        /// <param name="check">Булево - Значение пометки</param>
        [ContextMethod("ЗаполнитьПометки", "FillChecks")]
        public void FillChecks(bool check)
        {
            foreach (var item in _items)
            {
                item.Check = check;
            }
        }

        /// <summary>
        /// Получить индекс указанного элемента
        /// </summary>
        /// <param name="item">ЭлементСпискаЗначений - Элемент списка значений, для которого необходимо определить индекс</param>
        /// <returns>Число - Индекс в списке, если не найдено возвращает -1</returns>
        [ContextMethod("Индекс", "IndexOf")]
        public int IndexOf(ValueListItem item)
        {
            return _items.IndexOf(item);
        }

        /// <summary>
        /// Осуществляет поиск значения в списке
        /// </summary>
        /// <param name="val">Произвольный - Искомое значение</param>
        /// <returns>ЭлементСпискаЗначений - если элемент найден, иначе Неопределено</returns>
        [ContextMethod("НайтиПоЗначению", "FindByValue")]
        public IValue FindByValue(IValue val)
        {
            var item = _items.FirstOrDefault(x => x.Value.Equals(val));
            if(item == null)
                return ValueFactory.Create();

            return item;
        }

        private int IndexByValue(IValue item)
        {
            item = item.GetRawValue();

            int index;

            if (item is ValueListItem)
            {
                index = IndexOf(item as ValueListItem);
                if (index == -1)
                    throw new RuntimeException("Элемент не принадлежит списку значений");
            }
            else
            {
                try
                {
                    index = decimal.ToInt32(item.AsNumber());
                }
                catch (RuntimeException)
                {
                    throw RuntimeException.InvalidArgumentType();
                }

                if (index < 0 || index >= _items.Count())
                    throw new RuntimeException("Значение индекса выходит за пределы диапазона");
            }

            return index;
        }

        /// <summary>
        /// Сдвигает элемент на указанное количество позиций.
        /// </summary>
        /// <param name="item">
        /// ЭлементСпискаЗначений - Элемент, который сдвигаем
        /// Число - Индекс сдвигаемого элемента
        /// </param>
        /// <param name="offset">Количество позиций, на которое сдвигается элемент. Если значение положительное - сдвиг вниз, иначе вверх</param>
        [ContextMethod("Сдвинуть", "Move")]
        public void Move(IValue item, int offset)
        {
            int index_source = IndexByValue(item);

            int index_dest = index_source + offset;

            if (index_dest < 0 || index_dest >= _items.Count())
                throw RuntimeException.InvalidNthArgumentValue(2);

            ValueListItem itemObject = _items[index_source];

            if (index_source < index_dest)
            {
                _items.Insert(index_dest + 1, itemObject);
                _items.RemoveAt(index_source);
            }
            else
            {
                _items.RemoveAt(index_source);
                _items.Insert(index_dest, itemObject);
            }
        }

        /// <summary>
        /// Создает копию списка значений
        /// </summary>
        /// <returns>СписокЗначений</returns>
        [ContextMethod("Скопировать", "Copy")]
        public ValueListImpl Copy()
        {
            var newList = new ValueListImpl();
            foreach (var item in _items)
            {
                newList.Add(item.Value, item.Presentation, item.Check, item.Picture);
            }

            return newList;
        }

        /// <summary>
        /// Сортирует элементы в списке по порядку значений.
        /// </summary>
        /// <param name="direction">НаправлениеСортировки (необязательный) - Направление сортировки элементов. По умолчанию - по возрастанию.</param>
        [ContextMethod("СортироватьПоЗначению", "SortByValue")]
        public void SortByValue(SortDirectionEnum? direction = null)
        {
            if (direction == null || direction == SortDirectionEnum.Asc)
            {
                _items.Sort((x, y) => SafeCompare(x.Value, y.Value));
            }
            else
            {
                _items.Sort((x, y) => SafeCompare(y.Value, x.Value));
            }
        }

        private int SafeCompare(IValue x, IValue y)
        {
            try
            {
                return x.CompareTo(y);
            }
            catch(RuntimeException)
            {
                // Сравнение типов не поддерживается
                return x.AsString().CompareTo(y.AsString());
            }
        }

        /// <summary>
        /// Сортирует элементы в списке по порядку строкового представления.
        /// </summary>
        /// <param name="direction">НаправлениеСортировки (необязательный) - Направление сортировки элементов. По умолчанию - по возрастанию.</param>
        [ContextMethod("СортироватьПоПредставлению", "SortByPresentation")]
        public void SortByPresentation(SortDirectionEnum? direction = null)
        {
            if (direction == null || direction == SortDirectionEnum.Asc)
            {
                _items.Sort((x, y) => x.Presentation.CompareTo(y.Presentation));
            }
            else
            {
                _items.Sort((x, y) => y.Presentation.CompareTo(x.Presentation));
            }
        }

        /// <summary>
        /// Удаляет элемент из списка
        /// </summary>
        /// <param name="item">
        /// ЭлементСпискаЗначений - Удаляемый элемент
        /// Число - Индекс удаляемого элемента
        /// </param>
        [ContextMethod("Удалить", "Delete")]
        public void Delete(IValue item)
        {
            int indexSource = IndexByValue(item);

            _items.RemoveAt(indexSource);
        }

        #region Collection Context

        [ContextMethod("Количество", "Count")]
        public override int Count()
        {
            return _items.Count;
        }

        public override IEnumerator<ValueListItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion

        [ScriptConstructor]
        public static ValueListImpl Constructor()
        {
            return new ValueListImpl();
        }

    }
}
