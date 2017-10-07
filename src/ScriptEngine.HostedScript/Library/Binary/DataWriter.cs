/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;



/// <summary>
/// 
/// Объект предназначен для удобной записи различных типов данных в приемник.
/// Необходимо соблюдать следующий порядок работы с данным объектом:
/// 
///  - Создать или получить приемник даных.
///  - Вызвать из приемника экземпляр объекта ЗаписьДанных.
///  - Выполнить требуемые действия с помощью объекта.
///  - Закрыть экземпляр объекта ЗаписьДанных.
/// При необходимости использовать другие методы для работы с данными, требуется сначала закрыть экземпляр объекта ЗаписьДанных с помощью метода Закрыть, выполнить необходимые действия над данными, установить требуемую позицию для чтения из приемника и создать новый экземпляр ЗаписьДанных.
/// </summary>
[ContextClass("ЗаписьДанных", "DataWriter")]
class DataWriter : AutoContext<DataWriter>
{

    private IValue _TextEncoding;
    private string _ConvertibleSplitterOfLines;
    private IValue _ByteOrder;
    private string _LineSplitter;

    /// <summary>
    /// 
    /// Создает объект ЗаписьДанных для записи в указанный файл. Если файл с таким именем не существует, он будет создан. Параметр <Дописать> определяет, будут ли данные записаны в начало или в конец файла.
    /// После завершения работы с объектом, до закрытия потока, переданного в конструктор, объект необходимо закрыть с помощью метода Закрыть или НачатьЗакрытие. При этом используемый файл будет закрыт автоматически.
    /// </summary>
    ///
    /// <param name="FileName">
    /// Имя файла, в который будет выполнена запись. </param>
    /// <param name="TextEncoding">
    /// Кодировка текста для создаваемого экземпляра ЗаписьДанных. Если не задана, то используется UTF-8.
    /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="ByteOrder">
    /// Порядок байтов, используемый по умолчанию для кодирования целых чисел при записи в поток.
    /// Значение по умолчанию: LittleEndian. </param>
    /// <param name="LineSplitter">
    /// Разделитель по умолчанию для строк, записываемых в поток. Если разделитель строк не задан, то используется строка ПС.
    /// Значение по умолчанию: ПС. </param>
    /// <param name="Append">
    /// Определяет, будут ли данные записаны в начало или в конец файла:
    /// 
    ///  - Если Истина, то при открытии существующего файла запись будет выполнена в конец файла.
    ///  - Иначе данные будут записываться с начала файла, перезаписывая существующие данные.
    /// Если заданный файл не существует, будет создан новый файл с указанным именем и значение параметра не повлияет на поведение конструктора.
    /// Значение по умолчанию: Ложь. </param>
    /// <param name="ConvertibleSplitterOfLines">
    /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
    /// Значение по умолчанию: ВК + ПС. </param>
    /// <param name="WriteBOM">
    /// Если в начало файла или потока требуется записать метку порядка байтов (BOM) для используемой кодировки текста, то данный параметр должен иметь значение Истина.
    /// Значение по умолчанию: Ложь. </param>
    ///
    [ScriptConstructor]
    public static IRuntimeContextInstance Constructor(string FileName, IValue TextEncoding = null, IValue ByteOrder = null, string LineSplitter = null, bool Append = false, string ConvertibleSplitterOfLines = null, bool WriteBOM = false)
    {
        return new DataWriter();
    }

    /// <summary>
    /// 
    /// Объект создается для записи в заданный поток.
    /// После завершения работы с объектом, до закрытия потока, переданного в конструктор, объект необходимо закрыть с помощью метода Закрыть или НачатьЗакрытие.
    /// </summary>
    ///
    /// <param name="Stream">
    /// Поток, в который производится запись данных. Типы: Поток (Stream), ПотокВПамяти (MemoryStream), ФайловыйПоток (FileStream) </param>
    /// <param name="TextEncoding">
    /// Устанавливает кодировку текста для создаваемого экземпляра ЗаписьДанных. Если не задано, используется кодировка UTF-8.
    /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="ByteOrder">
    /// Порядок байтов, используемый по умолчанию для кодирования целых чисел при записи в поток.
    /// Значение по умолчанию: LittleEndian. </param>
    /// <param name="LineSplitter">
    /// Разделитель по умолчанию для строк, записываемых в поток. Если разделитель строк не задан, то используется строка ПС.
    /// Значение по умолчанию: ПС. </param>
    /// <param name="ConvertibleSplitterOfLines">
    /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
    /// Значение по умолчанию: ВК + ПС. </param>
    /// <param name="WriteBOM">
    /// Если в начало файла или потока требуется записать метку порядка байтов (BOM) для используемой кодировки текста, то данный параметр должен иметь значение Истина.
    /// Значение по умолчанию: Ложь. </param>
    ///
    [ScriptConstructor]
    public static IRuntimeContextInstance Constructor1(IValue Stream, IValue TextEncoding = null, IValue ByteOrder = null, string LineSplitter = null, IValue ConvertibleSplitterOfLines = null, bool WriteBOM = false)
    {
        return new DataWriter();
    }

    /// <summary>
    /// 
    /// Кодировка текста по-умолчанию для данного экземпляра ЗаписьДанных.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// </summary>
    /// <value>КодировкаТекста (TextEncoding), Строка (String)</value>
    [ContextProperty("КодировкаТекста", "TextEncoding")]
    public IValue TextEncoding { get; set; }

    /// <summary>
    /// 
    /// Конвертируемый разделитель строк. Этот параметр влияет на поведение метода ЗаписатьСимволы.
    /// </summary>
    /// <value>Строка (String)</value>
    [ContextProperty("КонвертируемыйРазделительСтрок", "ConvertibleSplitterOfLines")]
    public string ConvertibleSplitterOfLines { get; set; }

    /// <summary>
    /// 
    /// Порядок байтов по умолчанию.
    /// </summary>
    /// <value>ПорядокБайтов (ByteOrder)</value>
    [ContextProperty("ПорядокБайтов", "ByteOrder")]
    public IValue ByteOrder { get; private set; }


    /// <summary>
    /// 
    /// Разделитель строк по-умолчанию. Это свойство влияет на поведение метода ЗаписатьСтроку.
    /// </summary>
    /// <value>Строка (String)</value>
    [ContextProperty("РазделительСтрок", "LineSplitter")]
    public string LineSplitter { get; set; }
    
    /// <summary>
    /// 
    /// Вызывает метод СброситьБуферы. Если целевой поток был создан при создании объекта ЗаписьДанных, целевой поток также закрывается.
    /// </summary>
    ///
    ///
    [ContextMethod("Закрыть", "Close")]
    public void Close()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Записывает данные в целевой поток.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Запись двоичных данных
    /// </remarks>
    ///
    /// <param name="BinaryData">
    /// Записать экземпляр объекта ДвоичныеДанные в поток. </param>
    ///
    [ContextMethod("Записать", "Write")]
    public void Write(IValue BinaryDataOrReadResult)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Записывает один байт в целевой поток.
    /// </summary>
    ///
    /// <param name="Number">
    /// Целое число, которое будет записано в целевой поток. Значение числа должно находиться в диапазоне от 0 до 255. </param>
    ///
    [ContextMethod("ЗаписатьБайт", "WriteByte")]
    public void WriteByte(int Number)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Записать байты из буфера двоичных данных в целевой поток.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Запись буфера
    /// </remarks>
    ///
    /// <param name="Buffer">
    /// Буфер двоичных данных, все байты которого будут записаны в целевой поток. </param>
    ///
    [ContextMethod("ЗаписатьБуферДвоичныхДанных", "WriteBinaryDataBuffer")]
    public void WriteBinaryDataBuffer(IValue Buffer)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Записать байты из буфера двоичных данных в целевой поток.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Запись части байтов из буфера
    /// </remarks>
    ///
    /// <param name="Buffer">
    /// Буфер двоичных данных, который используется в качестве источника данных для записи в целевой поток. </param>
    /// <param name="PositionInBuffer">
    /// Позиция в буфере, начиная с которой выполняется чтение байтов для записи в целевой поток. </param>
    /// <param name="Number">
    /// Количество байтов, которые требуется записать в целевой поток. </param>
    ///
    [ContextMethod("ЗаписатьБуферДвоичныхДанных", "WriteBinaryDataBuffer")]
    public void WriteBinaryDataBuffer(IValue Buffer, int PositionInBuffer, int Number)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Записывает символы заданной строки в целевой поток.
    /// </summary>
    ///
    /// <param name="Line">
    /// Строка, символы которой будут записаны в поток. </param>
    /// <param name="Encoding">
    /// Определяет кодировку текста для записи строки. Если не установлена, используется кодировка, заданная для данного объекта ЗаписьДанных.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    ///
    [ContextMethod("ЗаписатьСимволы", "WriteChars")]
    public void WriteChars(string Line, IValue Encoding = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Записывает строку в целевой поток.
    /// Сначала записываются все символы строки, затем - разделитель строк.
    /// </summary>
    ///
    /// <param name="Line">
    /// Строка, которая будет записана в поток. </param>
    /// <param name="Encoding">
    /// Определяет кодировку текста для записи строки. Если не установлена, используется кодировка, заданная для данного объекта ЗаписьДанных.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="LineSplitter">
    /// Указывает строку, являющуюся разделителем строк в потоке после записи символов строк. Если параметр не указан, используется разделитель строк, указанный для текущего экземпляра объекта ЗаписьДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    [ContextMethod("ЗаписатьСтроку", "WriteLine")]
    public void WriteLine(string Line, IValue Encoding = null, string LineSplitter = null)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Записывает 16-разрядное число в целевой поток.
    /// </summary>
    ///
    /// <param name="Number">
    /// Число, которое будет записано в целевой поток.
    /// Значение числа должно находиться в диапазоне от 0 до 65535. </param>
    /// <param name="ByteOrder">
    /// Порядок байтов, который будет использован для кодировки числа при записи. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра объекта ЗаписьДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    [ContextMethod("ЗаписатьЦелое16", "WriteInt16")]
    public void WriteInt16(int Number, IValue ByteOrder = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Записать целое 32-битное число в целевой поток.
    /// </summary>
    ///
    /// <param name="Number">
    /// Целое число, которое будет записано в целевой поток. Значение числа должно находиться в диапазоне от 0 до 2^32-1. </param>
    /// <param name="ByteOrder">
    /// Порядок байтов, который будет использован для кодировки числа при записи. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра объекта ЗаписьДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    [ContextMethod("ЗаписатьЦелое32", "WriteInt32")]
    public void WriteInt32(int Number, IValue ByteOrder = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Записывает целое 16-битное число в целевой поток.
    /// </summary>
    ///
    /// <param name="Number">
    /// Целое число, которое будет записано в целевой поток. Значение числа должно находиться в диапазоне от 0 до 2^64-1. </param>
    /// <param name="ByteOrder">
    /// Порядок байтов, который будет использован для кодировки числа при записи. Если не установлен, то будет использован порядок байтов, заданный для текущего экземпляра объекта ЗаписьДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    [ContextMethod("ЗаписатьЦелое64", "WriteInt64")]
    public void WriteInt64(int Number, IValue ByteOrder = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Сбрасывает все внутренние буферы в целевой поток, после чего вызывает метод СброситьБуферы целевого потока.
    /// </summary>
    ///
    [ContextMethod("СброситьБуферы", "Flush")]
    public void Flush()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Возвращает целевой поток, в который выполняется запись.
    /// </summary>
    ///
    ///
    /// <returns name="Stream"/>
    ///
    [ContextMethod("ЦелевойПоток", "TargetStream")]
    public IValue TargetStream()
    {
        throw new NotImplementedException();
    }

}
