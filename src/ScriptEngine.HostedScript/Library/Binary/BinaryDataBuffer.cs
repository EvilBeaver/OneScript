
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;



/// <summary>
/// 
/// Коллекция байтов фиксированного размера с возможностью произвольного доступа и изменения по месту.
/// Размер буфера формально не ограничен, но поскольку все данные буфера полностью находятся в оперативной памяти, при попытке создать буфер слишком большого размера доступной памяти может оказаться недостаточно, в результате чего будет вызвано исключение. Поэтому при работе с буферами двоичных данных необходимо соотносить их размер с доступным объемом оперативной памяти.
/// При создании буфера можно указать порядок байтов, который будет использован для операций с целыми числами. При этом если буфер не создан явно, а получен с помощью вызова метода другого объекта, то порядок байтов в полученном буфере будет унаследован от порядка байтов, заданного для того объекта, метод которого вызывается.
/// Например, если буфер получен с помощью вызова метода ПрочитатьВБуферДвоичныхДанных, то порядок байтов в полученном буфере будет равен значению свойства ПорядокБайтов.
/// Возможен также более сложный случай наследования порядка байтов. Если буфер получен с помощью вызова метода ПолучитьБуферДвоичныхДанных, то порядок байтов у полученного буфера будет выбираться из объекта ЧтениеДанных, из которого был получен объект РезультатЧтенияДанных. 
/// Порядок байтов, заданный для объекта ЧтениеДанных, будет использован во всех объектах, полученных на его основании.
/// </summary>
[ContextClass("БуферДвоичныхДанных", "BinaryDataBuffer")]
public class BinaryDataBuffer : AutoContext<BinaryDataBuffer>, ICollectionContext
{
    private bool _readOnly;
    private Stream _readWriteStream;

    private readonly byte[] _buffer;

    public BinaryDataBuffer(byte[] buffer, ByteOrderEnum byteOrder = ByteOrderEnum.LittleEndian)
    {
        _buffer = buffer;
        ByteOrder = byteOrder;
        Converter = new EndianBitConverter();
        Converter.IsLittleEndian = byteOrder == ByteOrderEnum.LittleEndian;
    }

    private EndianBitConverter Converter { get; set; }

    /// <param name="size">
    /// Размер буфера в байтах. </param>
    /// <param name="byteOrder">
    /// Порядок байтов.
    /// Значение по умолчанию: LittleEndian. </param>
    ///
    [ScriptConstructor]
    public static BinaryDataBuffer Constructor(long size, ByteOrderEnum byteOrder = ByteOrderEnum.LittleEndian)
    {
        return new BinaryDataBuffer(new byte[size], byteOrder);
    }

    public override IValue GetIndexedValue(IValue index)
    {
        return ValueFactory.Create(Get((int) index.AsNumber()));
    }

    public override void SetIndexedValue(IValue index, IValue val)
    {
        var idx = (int)index.AsNumber();
        _buffer[idx] = (byte)val.AsNumber();
    }

    /// <summary>
    /// 
    /// Текущий порядок байтов. Влияет на операции чтения и записи целых чисел в буфер.
    /// </summary>
    /// <value>ПорядокБайтов (ByteOrder)</value>
    [ContextProperty("ПорядокБайтов", "ByteOrder")]
    public ByteOrderEnum ByteOrder { get; set; }

    /// <summary>
    /// 
    /// Размер буфера в байтах.
    /// </summary>
    /// <value>Число (Number)</value>
    [ContextProperty("Размер", "Size")]
    public long Size => _buffer.LongLength;

    /// <summary>
    /// 
    /// Значение Истина указывает, что данный буфер предназначен только для чтения.
    /// </summary>
    /// <value>Булево (Boolean)</value>
    [ContextProperty("ТолькоЧтение", "ReadOnly")]
    public bool ReadOnly
    {
        get { return _readOnly; }

    }

    /// <summary>
    /// 
    /// Заменить значения, начиная с заданной позиции, значениями из заданного буфера.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, начиная с которой требуется записать содержимое буфера. </param>
    /// <param name="bytes">
    /// Байты, которыми нужно заполнить часть буфера. </param>
    /// <param name="number">
    /// Количество байт, которые требуется заменить. </param>
    ///
    [ContextMethod("Записать", "Write")]
    public void Write(long position, BinaryDataBuffer bytes, long number = 0)
    {
        Array.Copy(bytes._buffer, _buffer, number);
    }

    private byte[] GetBytes<T>(Converter<T, byte[]> converterOverload, T value, IValue byteOrder = null)
    {
        byte[] bytes;
        if (byteOrder == null)
            bytes = converterOverload(value);
        else
        {
            try
            {
                var order = (ByteOrderEnum)(object)byteOrder.GetRawValue();
                Converter.IsLittleEndian = order == ByteOrderEnum.LittleEndian;
                bytes = converterOverload(value);
            }
            catch (InvalidCastException)
            {
                throw RuntimeException.InvalidArgumentType();
            }
        }

        return bytes;
    }

    private void CopyBytes(int position, byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            _buffer[position + i] = bytes[i];
        }
    }

    /// <summary>
    /// 
    /// Записать целое 16-битное положительное число в заданную позицию.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, на которой требуется записать число. </param>
    /// <param name="value">
    /// Число, которое требуется записать.
    /// Если значение не помещается в 16 бит, будет вызвано исключение. </param>
    /// <param name="byteOrder">
    /// Порядок байтов, который будет использован для кодировки числа при записи в буфер. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра БуферДвоичныхДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    ///
    [ContextMethod("ЗаписатьЦелое16", "WriteInt16")]
    public void WriteInt16(int position, short value, IValue byteOrder = null)
    {
        var bytes = GetBytes(Converter.GetBytes, value, byteOrder);
        CopyBytes(position, bytes);
    }

    /// <summary>
    /// 
    /// Записать целое 32-битное положительное число в заданную позицию.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, на которой требуется записать число. </param>
    /// <param name="value">
    /// Число, которое требуется записать.
    /// Если значение не помещается в 32 бита, будет вызван исключение. </param>
    /// <param name="byteOrder">
    /// Порядок байтов, который будет использован для кодировки числа при записи в буфер. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра БуферДвоичныхДанных.
    /// Значение по умолчанию: Неопределено. </param>
    [ContextMethod("ЗаписатьЦелое32", "WriteInt32")]
    public void WriteInt32(int position, int value, IValue byteOrder = null)
    {
        var bytes = GetBytes(Converter.GetBytes, value, byteOrder);
        CopyBytes(position, bytes);
    }


    /// <summary>
    /// 
    /// Записать целое 64-битное положительное число в заданную позицию.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, на которой требуется записать число. </param>
    /// <param name="value">
    /// Число, которое требуется записать.
    /// Если значение не помещается в 64 бита, будет вызвано исключение. </param>
    /// <param name="byteOrder">
    /// Порядок байтов, который будет использован для кодировки числа при записи в буфер. Если не установлен, то используется порядок байтов, заданный для текущего экземпляра БуферДвоичныхДанных.
    /// Значение по умолчанию: Неопределено. </param>

    ///

    ///
    [ContextMethod("ЗаписатьЦелое64", "WriteInt64")]
    public void WriteInt64(int position, long value, IValue byteOrder = null)
    {
        var bytes = GetBytes(Converter.GetBytes, value, byteOrder);
        CopyBytes(position, bytes);
    }


    /// <summary>
    /// 
    /// Создает новый буфер, содержащий элементы текущего буфера в противоположном порядке.
    /// </summary>
    ///
    ///
    /// <returns name="BinaryDataBuffer">
    /// Коллекция байтов фиксированного размера с возможностью произвольного доступа и изменения по месту.
    /// Размер буфера формально не ограничен, но поскольку все данные буфера полностью находятся в оперативной памяти, при попытке создать буфер слишком большого размера доступной памяти может оказаться недостаточно, в результате чего будет вызвано исключение. Поэтому при работе с буферами двоичных данных необходимо соотносить их размер с доступным объемом оперативной памяти.
    /// При создании буфера можно указать порядок байтов, который будет использован для операций с целыми числами. При этом если буфер не создан явно, а получен с помощью вызова метода другого объекта, то порядок байтов в полученном буфере будет унаследован от порядка байтов, заданного для того объекта, метод которого вызывается.
    /// Например, если буфер получен с помощью вызова метода ПрочитатьВБуферДвоичныхДанных, то порядок байтов в полученном буфере будет равен значению свойства ПорядокБайтов.
    /// Возможен также более сложный случай наследования порядка байтов. Если буфер получен с помощью вызова метода ПолучитьБуферДвоичныхДанных, то порядок байтов у полученного буфера будет выбираться из объекта ЧтениеДанных, из которого был получен объект РезультатЧтенияДанных. 
    /// Порядок байтов, заданный для объекта ЧтениеДанных, будет использован во всех объектах, полученных на его основании.</returns>

    ///
    [ContextMethod("Перевернуть", "Reverse")]
    public BinaryDataBuffer Reverse()
    {
        var bytes = _buffer.Reverse().ToArray();
        return new BinaryDataBuffer(bytes, ByteOrder);
    }


    /// <summary>
    /// 
    /// Получает значение элемента на указанной позиции.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция элемента в буфере. Нумерация начинается с 0. </param>

    ///
    /// <returns name="Number">
    /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>

    ///
    [ContextMethod("Получить", "Get")]
    public int Get(int position)
    {
        return _buffer[position];
    }


    /// <summary>
    /// Создает новый буфер, использующий заданное количество байтов из исходного буфера, начиная с заданной позиции (нумерация с 0). Если количество не задано, то новый буфер является представлением элементов текущего буфера, начиная с заданного индекса и до конца.
    /// 
    /// НЕ РЕАЛИЗОВАН
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, начиная с которой будет создан новый буфер. </param>
    /// <param name="number">
    /// Количество байтов, которые требуется отобразить в срезе. Если на задано, то отображаются все байты от начала среза до конца исходного буфера.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="BinaryDataBuffer">
    /// </returns>

    ///
    [ContextMethod("ПолучитьСрез", "GetSlice")]
    public BinaryDataBuffer GetSlice(int position, IValue number = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Выполняет чтение байтов из буфера и помещает их в новый буфер.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, начиная с которой требуется прочитать байты. </param>
    /// <param name="number">
    /// Количество байтов, которое требуется прочитать. </param>

    ///
    /// <returns name="BinaryDataBuffer">
    /// Коллекция байтов фиксированного размера с возможностью произвольного доступа и изменения по месту.
    /// Размер буфера формально не ограничен, но поскольку все данные буфера полностью находятся в оперативной памяти, при попытке создать буфер слишком большого размера доступной памяти может оказаться недостаточно, в результате чего будет вызвано исключение. Поэтому при работе с буферами двоичных данных необходимо соотносить их размер с доступным объемом оперативной памяти.
    /// При создании буфера можно указать порядок байтов, который будет использован для операций с целыми числами. При этом если буфер не создан явно, а получен с помощью вызова метода другого объекта, то порядок байтов в полученном буфере будет унаследован от порядка байтов, заданного для того объекта, метод которого вызывается.
    /// Например, если буфер получен с помощью вызова метода ПрочитатьВБуферДвоичныхДанных, то порядок байтов в полученном буфере будет равен значению свойства ПорядокБайтов.
    /// Возможен также более сложный случай наследования порядка байтов. Если буфер получен с помощью вызова метода ПолучитьБуферДвоичныхДанных, то порядок байтов у полученного буфера будет выбираться из объекта ЧтениеДанных, из которого был получен объект РезультатЧтенияДанных. 
    /// Порядок байтов, заданный для объекта ЧтениеДанных, будет использован во всех объектах, полученных на его основании.</returns>

    ///
    [ContextMethod("Прочитать", "Read")]
    public BinaryDataBuffer Read(int position, int number)
    {
        var data = new byte[number];
        Array.Copy(_buffer, position, data, 0, number);
        return new BinaryDataBuffer(data, ByteOrder);
    }


    private T FromBytes<T>(Func<byte[],int,T> converterOverload, int position, IValue byteOrder = null) where T : struct
    {
        ByteOrderEnum order = ByteOrder;
        if (byteOrder != null)
            order = (ByteOrderEnum)(object)byteOrder.GetRawValue();
        Converter.IsLittleEndian = order == ByteOrderEnum.LittleEndian;

        return converterOverload(_buffer, position);
    }

    /// <summary>
    /// 
    /// Выполняет чтение целого 16-битного положительного числа на заданной позиции.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, на которой требуется прочитать число. </param>
    /// <param name="byteOrder">
    /// Порядок байтов, используемый при чтении числа.
    /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="Number"/>
    ///
    [ContextMethod("ПрочитатьЦелое16", "ReadInt16")]
    public short ReadInt16(int position, IValue byteOrder = null)
    {
        return FromBytes(Converter.ToInt16, position, byteOrder);
    }


    /// <summary>
    /// 
    /// Прочитать целое 32-битное положительное число на заданной позиции.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, на которой требуется прочитать число. </param>
    /// <param name="byteOrder">
    /// Порядок байтов, используемый при чтении числа.
    /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>

    ///
    /// <returns name="Number">
    /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>

    ///
    [ContextMethod("ПрочитатьЦелое32", "ReadInt32")]
    public int ReadInt32(int position, IValue byteOrder = null)
    {
        return FromBytes(Converter.ToInt32, position, byteOrder);
    }


    /// <summary>
    /// 
    /// Выполняет чтение целого 64-битного положительного числа на заданной позиции.
    /// </summary>
    ///
    /// <param name="position">
    /// Позиция, на которой требуется прочитать число. </param>
    /// <param name="byteOrder">
    /// Порядок байтов, используемый при чтении числа.
    /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="Number">
    /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>

    ///
    [ContextMethod("ПрочитатьЦелое64", "ReadInt64")]
    public long ReadInt64(int position, IValue byteOrder = null)
    {
        return FromBytes(Converter.ToInt64, position, byteOrder);
    }


    /// <summary>
    /// Разделить буфер на части по заданному разделителю.
    /// 
    /// НЕ РЕАЛИЗОВАН
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// По двоичному буферу
    /// </remarks>
    ///
    /// <param name="separator">
    /// Разделитель. </param>
    ///
    /// <returns name="Array"/>
    ///
    [ContextMethod("Разделить", "Split")]
    public IValue Split(IValue separator)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Создает копию массива.
    /// </summary>
    ///
    ///
    /// <returns name="BinaryDataBuffer"/>
    ///
    [ContextMethod("Скопировать", "Copy")]
    public BinaryDataBuffer Copy()
    {
        
    }


    /// <summary>
    /// 
    /// Создает новый буфер, содержащий элементы текущего буфера и, за ними, элементы заданного буфера.
    /// </summary>
    ///
    /// <param name="Buffer">
    /// Буфер, который будет соединен с исходным. </param>

    ///
    /// <returns name="BinaryDataBuffer">
    /// Коллекция байтов фиксированного размера с возможностью произвольного доступа и изменения по месту.
    /// Размер буфера формально не ограничен, но поскольку все данные буфера полностью находятся в оперативной памяти, при попытке создать буфер слишком большого размера доступной памяти может оказаться недостаточно, в результате чего будет вызвано исключение. Поэтому при работе с буферами двоичных данных необходимо соотносить их размер с доступным объемом оперативной памяти.
    /// При создании буфера можно указать порядок байтов, который будет использован для операций с целыми числами. При этом если буфер не создан явно, а получен с помощью вызова метода другого объекта, то порядок байтов в полученном буфере будет унаследован от порядка байтов, заданного для того объекта, метод которого вызывается.
    /// Например, если буфер получен с помощью вызова метода ПрочитатьВБуферДвоичныхДанных, то порядок байтов в полученном буфере будет равен значению свойства ПорядокБайтов.
    /// Возможен также более сложный случай наследования порядка байтов. Если буфер получен с помощью вызова метода ПолучитьБуферДвоичныхДанных, то порядок байтов у полученного буфера будет выбираться из объекта ЧтениеДанных, из которого был получен объект РезультатЧтенияДанных. 
    /// Порядок байтов, заданный для объекта ЧтениеДанных, будет использован во всех объектах, полученных на его основании.</returns>

    ///
    [ContextMethod("Соединить", "Concat")]
    public IValue Concat(IValue Buffer)
    {
        return null;
    }


    /// <summary>
    /// 
    /// Устанавливает значение элемента на заданной позиции (нумерация начинается с 0).
    /// </summary>
    ///
    /// <param name="Position">
    /// Позиция, на которую требуется поместить новое значение. </param>
    /// <param name="Value">
    /// Значение, которое требуется установить в заданную позицию буфера.
    /// Если значение больше 255 или меньше 0, будет выдана ошибка о неверном значении параметра. </param>

    ///

    ///
    [ContextMethod("Установить", "Set")]
    public void Set(int Position, int Value)
    {

    }


    /// <summary>
    /// 
    /// Переводит текущий буфер в режим "только для чтения". 
    /// Попытка изменить состояние буфера приведет к вызову исключения.
    /// </summary>
    ///

    ///

    ///
    [ContextMethod("УстановитьТолькоЧтение", "SetReadOnly")]
    public void SetReadOnly()
    {
        _readOnly = true;
    }

    public int Count()
    {
        return _buffer.Length;
    }

    public CollectionEnumerator GetManagedIterator()
    {
        return new CollectionEnumerator(GetEnumerator());
    }

    private IEnumerator<IValue> GetEnumerator()
    {
        for (long i = 0; i < _buffer.LongLength; i++)
        {
            yield return ValueFactory.Create(_buffer[i]);
        }
    }

    private BinaryWriter GetWriter()
    {
        return new BinaryWriter(InternalStream);
    }

    private Stream InternalStream
    {
        get
        {
            if(_readWriteStream == null)
                _readWriteStream = new MemoryStream(_buffer);

            return _readWriteStream;
        }
    }
}