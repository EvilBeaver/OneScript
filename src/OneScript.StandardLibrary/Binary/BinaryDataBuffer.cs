/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Binary
{
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
    public class BinaryDataBuffer : AutoCollectionContext<BinaryDataBuffer, IValue>
    {
        public BinaryDataBuffer(byte[] buffer, ByteOrderEnum byteOrder = ByteOrderEnum.LittleEndian)
        {
            Bytes = buffer;
            ByteOrder = byteOrder;
        }
    
        // для операций с содержимым буфера внутри 1Script
        //
        public byte[] Bytes { get; }

        /// <param name="size">
        /// Размер буфера в байтах. </param>
        /// <param name="byteOrder">
        /// Порядок байтов.
        /// Значение по умолчанию: LittleEndian. </param>
        ///
        [ScriptConstructor]
        public static BinaryDataBuffer Constructor(IValue size, IValue byteOrder = null)
        {
            var orderValue = byteOrder == null ? ByteOrderEnum.LittleEndian : ContextValuesMarshaller.ConvertParam<ByteOrderEnum>(byteOrder);

            return new BinaryDataBuffer(
                new byte[ContextValuesMarshaller.ConvertParam<int>(size)],
                orderValue);
        }

        public override IValue GetIndexedValue(IValue index)
        {
            return ValueFactory.Create(Get((int) index.AsNumber()));
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            ThrowIfReadonly();

            int value = (int)val.AsNumber();
            if (value < byte.MinValue || value > byte.MaxValue)
                throw RuntimeException.InvalidArgumentValue();

            var idx = (int)index.AsNumber();
            Bytes[idx] = (byte) value;
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
        public long Size => Bytes.LongLength;

        /// <summary>
        /// 
        /// Значение Истина указывает, что данный буфер предназначен только для чтения.
        /// </summary>
        /// <value>Булево (Boolean)</value>
        [ContextProperty("ТолькоЧтение", "ReadOnly")]
        public bool ReadOnly { get; private set; }

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
        public void Write(int position, BinaryDataBuffer bytes, int number = 0)
        {
            ThrowIfReadonly();

            if (number == 0)
                Array.Copy(bytes.Bytes, 0, Bytes, position, bytes.Bytes.Length);
            else
                Array.Copy(bytes.Bytes, 0, Bytes, position, number);
        }

        private byte[] GetBytes<T>(T value, Converter<T, byte[]> leConverter, Converter<T, byte[]> beConverter, IValue byteOrder = null)
        {
            ByteOrderEnum workByteOrder;
            if (byteOrder == null)
                workByteOrder = ByteOrder;
            else
            {
                var enumVal = byteOrder.GetRawValue() as IObjectWrapper;
                if (enumVal == null)
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));

                try
                {
                    workByteOrder = (ByteOrderEnum)enumVal.UnderlyingObject;
                }
                catch (InvalidCastException)
                {
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));
                }
            }

            var converter = workByteOrder == ByteOrderEnum.BigEndian ? beConverter : leConverter;
            return converter(value);
        }

        private void CopyBytes(int position, byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                Bytes[position + i] = bytes[i];
            }
        }

        private static ulong AsUnsignedLong( IValue value, ulong maxValue=ulong.MaxValue )
        {
            if (value.SystemType != BasicTypes.Number)
                throw RuntimeException.InvalidArgumentType(2,nameof(value));

            var number = value.AsNumber();
            if ( number < 0 || number > maxValue || number != (ulong)number )
                throw RuntimeException.InvalidArgumentValue(number);

            return (ulong)number;
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
        public void WriteInt16(int position, IValue value, IValue byteOrder = null)
        {
            ThrowIfReadonly();

            var number = AsUnsignedLong(value,ushort.MaxValue);
            var bytes = GetBytes( (ushort)number, BitConversionFacility.LittleEndian.GetBytes, BitConversionFacility.BigEndian.GetBytes, byteOrder);
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
        public void WriteInt32(int position, IValue value, IValue byteOrder = null)
        {
            ThrowIfReadonly();

            var number = AsUnsignedLong(value, uint.MaxValue);
            var bytes = GetBytes((uint)number, BitConversionFacility.LittleEndian.GetBytes, BitConversionFacility.BigEndian.GetBytes, byteOrder);
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
        public void WriteInt64(int position, IValue value, IValue byteOrder = null)
        {
            ThrowIfReadonly();

            var number = AsUnsignedLong(value);
            var bytes = GetBytes( number, BitConversionFacility.LittleEndian.GetBytes, BitConversionFacility.BigEndian.GetBytes, byteOrder);
            CopyBytes(position, bytes);
        }
        
        private void WriteBitwiseOp(int position, BinaryDataBuffer buffer, int number, Func<byte, byte, byte> op)
        {
            if(position < 0)
                throw new IndexOutOfRangeException("Значение индекса выходит за границы диапазона");

            try
            {
                var bytesToCopy = (number == 0 ? buffer.Bytes.Length : number);
                for (int i = 0; i < bytesToCopy; i++)
                    Bytes[i + position] = op(Bytes[i + position], buffer.Bytes[i]);
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException("Переполнение при работе с буфером");
            }
        }
        
        /// <summary>
        /// 
        /// Объединить заданное количество байтов, начиная с указанной позиции с байтами из заданного буфера
        /// с использованием побитового И.
        /// Если количество байтов не указано, то объединяются все байты до конца буфера.
        /// </summary>
        ///
        /// <param name="position">
        /// Начальная позиция в буфере. </param>
        /// <param name="bytes">
        /// Буфер, с которым выполняется объединение. </param>
        /// <param name="number">
        /// Количество байт, которые требуется объединить. </param>
        ///
        [ContextMethod("ЗаписатьПобитовоеИ", "WriteBitwiseAnd")]
        public void WriteBitwiseAnd(int position, BinaryDataBuffer bytes, int number = 0)
        {
            ThrowIfReadonly();
            WriteBitwiseOp(position, bytes, number, ((i, j) => (byte)(i & j)));
        }
        
        /// <summary>
        /// 
        /// Объединить заданное количество байтов, начиная с указанной позиции с байтами из заданного буфера
        /// с использованием побитового И НЕ.
        /// Если количество байтов не указано, то объединяются все байты до конца буфера.
        /// </summary>
        ///
        /// <param name="position">
        /// Начальная позиция в буфере. </param>
        /// <param name="bytes">
        /// Буфер, с которым выполняется объединение. </param>
        /// <param name="number">
        /// Количество байт, которые требуется объединить. </param>
        ///
        [ContextMethod("ЗаписатьПобитовоеИНе", "WriteBitwiseAndNot")]
        public void WriteBitwiseAndNot(int position, BinaryDataBuffer bytes, int number = 0)
        {
            ThrowIfReadonly();
            WriteBitwiseOp(position, bytes, number, ((i, j) => (byte)(i & ~j)));
        }
        
        /// <summary>
        /// 
        /// Объединить заданное количество байтов, начиная с указанной позиции с байтами из заданного буфера
        /// с использованием побитового ИЛИ.
        /// Если количество байтов не указано, то объединяются все байты до конца буфера.
        /// </summary>
        ///
        /// <param name="position">
        /// Начальная позиция в буфере. </param>
        /// <param name="bytes">
        /// Буфер, с которым выполняется объединение. </param>
        /// <param name="number">
        /// Количество байт, которые требуется объединить. </param>
        ///
        [ContextMethod("ЗаписатьПобитовоеИли", "WriteBitwiseOr")]
        public void WriteBitwiseOr(int position, BinaryDataBuffer bytes, int number = 0)
        {
            ThrowIfReadonly();
            WriteBitwiseOp(position, bytes, number, ((i, j) => (byte)(i | j)));
        }
        
        /// <summary>
        /// 
        /// Объединить заданное количество байтов, начиная с указанной позиции с байтами из заданного буфера
        /// с использованием побитового ИСКЛЮЧИТЕЛЬНОГО ИЛИ (XOR).
        /// Если количество байтов не указано, то объединяются все байты до конца буфера.
        /// </summary>
        ///
        /// <param name="position">
        /// Начальная позиция в буфере. </param>
        /// <param name="bytes">
        /// Буфер, с которым выполняется объединение. </param>
        /// <param name="number">
        /// Количество байт, которые требуется объединить. </param>
        ///
        [ContextMethod("ЗаписатьПобитовоеИсключительноеИли", "WriteBitwiseXor")]
        public void WriteBitwiseXor(int position, BinaryDataBuffer bytes, int number = 0)
        {
            ThrowIfReadonly();
            WriteBitwiseOp(position, bytes, number, ((i, j) => (byte)(i ^ j)));
        }

        /// <summary>
        /// 
        /// Создает новый буфер, содержащий элементы текущего буфера в противоположном порядке.
        /// </summary>
        ///
        ///
        /// <returns name="BinaryDataBuffer"/>
        ///
        [ContextMethod("Перевернуть", "Reverse")]
        public BinaryDataBuffer Reverse()
        {
            var bytes = Bytes.Reverse().ToArray();
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
            return Bytes[position];
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
        /// <returns name="BinaryDataBuffer"/>
        ///
        [ContextMethod("Прочитать", "Read")]
        public BinaryDataBuffer Read(int position, int number)
        {
            var data = new byte[number];
            Array.Copy(Bytes, position, data, 0, number);
            return new BinaryDataBuffer(data, ByteOrder);
        }
    
        private T FromBytes<T>(int position, Func<byte[], int, T> leConverter, Func<byte[], int, T> beConverter, IValue byteOrder = null)
        {
            ByteOrderEnum workByteOrder;
            if (byteOrder == null)
                workByteOrder = ByteOrder;
            else
            {
                var enumVal = byteOrder.GetRawValue() as IObjectWrapper;
                if (enumVal == null)
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));

                try
                {
                    workByteOrder = (ByteOrderEnum)enumVal.UnderlyingObject;
                }
                catch (InvalidCastException)
                {
                    throw RuntimeException.InvalidArgumentType(nameof(byteOrder));
                }
            }

            var converter = workByteOrder == ByteOrderEnum.BigEndian ? beConverter : leConverter;
            return converter(Bytes, position);
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
        public int ReadInt16(int position, IValue byteOrder = null)
        {
            return FromBytes(position, BitConversionFacility.LittleEndian.ToInt16, BitConversionFacility.BigEndian.ToInt16, byteOrder);
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
        public uint ReadInt32(int position, IValue byteOrder = null)
        {
            return FromBytes(position, BitConversionFacility.LittleEndian.ToUInt32, BitConversionFacility.BigEndian.ToUInt32, byteOrder);
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
        public ulong ReadInt64(int position, IValue byteOrder = null)
        {
            return FromBytes(position, BitConversionFacility.LittleEndian.ToUInt64, BitConversionFacility.BigEndian.ToUInt64, byteOrder);
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
        public ArrayImpl Split(IValue separator)
        {
            var buffers = ParseParam(separator);

            // Функция поиска требует, чтобы буферы были в порядке убывания размера
            buffers.Sort((a, b) => b.Bytes.LongLength.CompareTo(a.Bytes.LongLength));
            return SplitBuffer(buffers.ToArray());
        }

        private static List<BinaryDataBuffer> ParseParam(IValue separator)
        {
            var rawSeparator = separator?.GetRawValue();
            switch (rawSeparator)
            {
                case BinaryDataBuffer buffer:
                    return new List<BinaryDataBuffer> { CheckedBuffer(buffer) };

                case ArrayImpl array:
                {
                    var buffers = new List<BinaryDataBuffer>();

                    foreach (var element in array)
                    {
                        buffers.AddRange(ParseParam(element));
                    }

                    return buffers;
                }

                default:
                    throw RuntimeException.InvalidArgumentType();
            }
        }

        private static BinaryDataBuffer CheckedBuffer(BinaryDataBuffer buffer)
        {
            if (buffer.Size == 0)
            {
                throw RuntimeException.InvalidArgumentValue();
            }

            return buffer;
        }

        private ArrayImpl SplitBuffer(BinaryDataBuffer[] splitter)
        {
            var result = new List<BinaryDataBuffer>();
            long start = 0;
            var foundPosition = FindFirst(splitter, start);
            while (foundPosition.pos != -1)
            {
                var length = foundPosition.pos - start;
                result.Add(new BinaryDataBuffer(Copy(start, length), ByteOrder));
                start = foundPosition.pos + foundPosition.buffer.Size;
                foundPosition = FindFirst(splitter, start);
            }

            // хвостовой элемент
            result.Add(new BinaryDataBuffer(Copy(start, Bytes.LongLength - start)));
            return new ArrayImpl(result);
        }

        /// <summary>
        /// Ищет ближайшее вхождение любого из буферов. Если на одной позиции находятся два и более буфера, берется бОльший.
        /// </summary>
        /// <param name="buffers">Массив искомых буферов</param>
        /// <param name="start">Начальная позиция поиска</param>
        /// <returns>Буфер и позиция или null, если нет вхождений</returns>
        private (BinaryDataBuffer buffer, long pos) FindFirst(BinaryDataBuffer[] buffers, long start)
        {
            var maxI = Size - buffers[buffers.Length - 1].Size;
            for (var i = start; i < maxI; i++)
            {
                foreach (var expectedBuffer in buffers)
                {
                    if (SubsequenceEquals(Bytes, i, expectedBuffer.Bytes))
                    {
                        return (expectedBuffer, i);
                    }
                }
            }

            return (null, -1);
        }

        private byte[] Copy(long start, long length)
        {
            if (length == 0) return Array.Empty<byte>();
            var partition = new byte[length];
            Array.Copy(Bytes, start, partition, 0, length);
            return partition;
        }

        private static bool SubsequenceEquals(byte[] sequence, long start, byte[] subsequence)
        {
            for (long j = 0; j < subsequence.LongLength; j++)
            {
                if (subsequence[j] != sequence[start + j])
                {
                    return false;
                }
            }

            return true;
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
            byte[] copy = new byte[Bytes.Length];
            Array.Copy(Bytes, copy, Bytes.Length);
            return new BinaryDataBuffer(copy, ByteOrder);
        }


        /// <summary>
        /// 
        /// Создает новый буфер, содержащий элементы текущего буфера и, за ними, элементы заданного буфера.
        /// </summary>
        ///
        /// <param name="buffer">
        /// Буфер, который будет соединен с исходным. </param>
        ///
        /// <returns name="BinaryDataBuffer"/>
        ///
        [ContextMethod("Соединить", "Concat")]
        public BinaryDataBuffer Concat(BinaryDataBuffer buffer)
        {
            var source = buffer.Bytes;
            var totalLength = Bytes.Length + source.Length;
            var joinedArray = new byte[totalLength];
            Array.Copy(Bytes, joinedArray, Bytes.Length);
            Array.Copy(source, 0, joinedArray, Bytes.Length, source.Length);

            return new BinaryDataBuffer(joinedArray, ByteOrder);
        }


        /// <summary>
        /// 
        /// Устанавливает значение элемента на заданной позиции (нумерация начинается с 0).
        /// </summary>
        ///
        /// <param name="position">
        /// Позиция, на которую требуется поместить новое значение. </param>
        /// <param name="value">
        /// Значение, которое требуется установить в заданную позицию буфера.
        /// Если значение больше 255 или меньше 0, будет выдана ошибка о неверном значении параметра. </param>
        [ContextMethod("Установить", "Set")]
        public void Set(int position, IValue value)
        {
            ThrowIfReadonly();

            Bytes[position] = (byte)AsUnsignedLong(value, byte.MaxValue);
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
            ReadOnly = true;
        }

        public override int Count()
        {
            return Bytes.Length;
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            for (long i = 0; i < Bytes.LongLength; i++)
            {
                yield return ValueFactory.Create(Bytes[i]);
            }
        }

        public void ThrowIfReadonly()
        {
            if (ReadOnly)
                throw new RuntimeException("Буфер находится в режиме \"Только чтение\"");
        }
    }
}
