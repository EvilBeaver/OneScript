/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;
using OneScript.StandardLibrary.TypeDescriptions;
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

        public override bool IsIndexed => true;

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
                throw IndexedIsReadonlyException();

            base.SetIndexedValue(index, val);
        }

        /// <summary>
        /// Определяет тип для значений, которые могут храниться в элементах данного списка значений 
        /// </summary>
        /// <value>ОписаниеТипов</value>
        [ContextProperty("ТипЗначения", "ValueType")]
        public TypeDescription ListValueType { get; set; } = new();


        ValueListImpl _availableValues;

        /// <summary>
        /// Ограничивает допустимые значения для элементов данного списка значений. 
        /// Возможные значения: Неопределено - ограничения отсутствуют,
        /// СписокЗначений - список допустимых значений
        /// </summary>
        /// <remarks>
        /// Проверка допустимости добавляемых значений производится после приведения к указанным в ТипЗначения (если есть)
        /// </remarks>
        /// <value>СписокЗначений или Неопределено</value>
        [ContextProperty("ДоступныеЗначения", "AvailableValues")]
        public IValue AvailableValues
        {
            get
            {
                if (_availableValues != null)
                    return _availableValues;

                return BslUndefinedValue.Instance;
            }

            set
            {
                switch (value)
                {
                    case BslUndefinedValue:
                        _availableValues = null;
                        break;

                    case ValueListImpl vl:
                        _availableValues = vl;
                        break;

                    default: throw RuntimeException.InvalidArgumentType();
                }
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
            if (numericIndex < 0 || numericIndex >= _items.Count)
                throw RuntimeException.IndexOutOfRange();

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
        public ValueListItem Add(IValue value = null, string presentation = null, bool check = false, IValue picture = null)
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
        public ValueListItem Insert(int index, IValue value = null, string presentation = null, bool check = false, IValue picture = null)
        {
            if (index < 0 || index > _items.Count)
                throw RuntimeException.IndexOutOfRange();

            var newItem = CreateNewListItem(value, presentation, check, picture);
            _items.Insert(index, newItem);
            
            return newItem;
        }

        private ValueListItem CreateNewListItem(IValue value, string presentation, bool check, IValue picture)
        {
            var newValue = ListValueType.AdjustValue(value);
            if (_availableValues is not null)
            {
                var foundItem = _availableValues.FindByValue(newValue);
                newValue = foundItem is ValueListItem li ? li.Value : ListValueType.AdjustValue();
            }

            return new ValueListItem
            {
                Value = newValue,
                Presentation = presentation,
                Check = check,
                Picture = picture
            };
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
            var item = _items.FirstOrDefault(x => x.Value.StrictEquals(val));
            if(item == null)
                return ValueFactory.Create();

            return item;
        }

        private int IndexByValue(BslValue item)
        {
            int index;

            if (item is ValueListItem listItem)
            {
                index = IndexOf(listItem);
                if (index == -1)
                    throw ElementDoesntBelongException();
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

                if (index < 0 || index >= _items.Count)
                    throw RuntimeException.IndexOutOfRange();
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
        public void Move(BslValue item, int offset)
        {
            int index_source = IndexByValue(item);

            int index_dest = index_source + offset;

            if (index_dest < 0 || index_dest >= _items.Count)
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
        public void SortByValue(IBslProcess process, SortDirectionEnum? direction = null)
        {
            _items.Sort( new ItemComparator(process, direction == null || direction == SortDirectionEnum.Asc) );
        }

        private class ItemComparator : IComparer<ValueListItem>
        {
            readonly GenericIValueComparer _comparer;
            readonly int _direction;

            public ItemComparator(IBslProcess process, bool ascending = true)
            {
                _comparer = new GenericIValueComparer(process);
                _direction = ascending ? 1 : -1;
            }

            public int Compare(ValueListItem x, ValueListItem y)
            {
                return _comparer.Compare(x.Value, y.Value) * _direction;
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
        public void Delete(BslValue item)
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

        public override string ToString() => string.Join("; ", _items);

        [ScriptConstructor]
        public static ValueListImpl Constructor()
        {
            return new ValueListImpl();
        }

        public static RuntimeException IndexedIsReadonlyException()
        {
            return new("Индексированное значение доступно только для чтения", "Indexed value is read-only");
        }

        public static RuntimeException ElementDoesntBelongException()
        {
            return new("Элемент не принадлежит списку значений", "Element does not belong to values list");
        }

    }
}
