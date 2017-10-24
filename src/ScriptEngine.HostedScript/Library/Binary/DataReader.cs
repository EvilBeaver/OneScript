/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Text;
using ScriptEngine.HostedScript.Library.Binary;

/// <summary>
/// 
/// Объект предназначен для чтения различных типов данных из источника данных. В качестве источника могут выступать Поток, Файл или ДвоичныеДанные.
/// Необходимо соблюдать следующий порядок работы с данным объектом:
/// 
///  - Создать объект ЧтениеДанных на основании источника данных.
///  - Выполнить требуемые действия с помощью объекта.
///  - Закрыть экземпляр объекта ЧтениеДанных.
/// При необходимости использовать другие методы для работы с источником данных, требуется сначала закрыть экземпляр объекта ЧтениеДанных с помощью метода Закрыть, выполнить необходимые действия над источником, установить требуемую позицию для чтения из источника и создать новый экземпляр ЧтениеДанных.
/// </summary>
[ContextClass("ЧтениеДанных", "DataReader")]
class DataReader : AutoContext<DataReader>
{

    private readonly Encoding _textEncoding;
    private string _convertibleSplitterOfLines;
    private ByteOrderEnum _byteOrder;
    private string _lineSplitter;
    private bool _ReadCompleted;

    public DataReader(IStreamWrapper streamImpl)
    {
    }


    /// <summary>
    /// 
    /// Создает объект для чтения из заданного объекта ДвоичныеДанные.
    /// После завершения работы с объектом ЧтениеДанных до того, как будет закрыт поток, переданный в конструктор, объект следует закрыть с помощью метода Закрыть или НачатьЗакрытие.
    /// </summary>
    ///
    /// <param name="binaryData">
    /// Экземпляр объекта ДвоичныеДанные, из которого будет выполнено чтение. </param>
    /// <param name="textEncoding">
    /// Определяет кодировку текста, используемую для чтения данных. По-умолчанию используется кодировка UTF-8.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// 
    /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="byteOrder">
    /// Порядок байтов, используемый для декодирования целых чисел при чтении из потока.
    /// Значение по умолчанию: LittleEndian. </param>
    /// <param name="lineSplitter">
    /// Определяет строку, разделяющую строки в двоичных данных.
    /// Значение по умолчанию: Неопределено. </param>
    /// <param name="convertibleSplitterOfLines">
    /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
    /// Значение по умолчанию: ВК + ПС. </param>
    [ScriptConstructor(Name = "На основании двоичных данных")]
    public static IRuntimeContextInstance Constructor(IValue binaryData, IValue textEncoding = null, IValue byteOrder = null, string lineSplitter = null, string convertibleSplitterOfLines = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Создает объект чтения из заданного файла.
    /// При этом будет файл, указанный в параметре ИмяФайла, будет автоматически открыт на чтение. 
    /// Если файл с таким именем не существует, будет сгенерировано исключение.
    /// 
    /// После завершения работы с объектом ЧтениеДанных до того, как будет закрыт поток, переданный в конструктор, объект следует закрыть с помощью метода Закрыть или НачатьЗакрытие. При этом файл, указанный в параметре &lt;ИмяФайла&gt;, будет автоматически закрыт.
    /// </summary>
    ///
    /// <param name="fileName">
    /// Имя файла, из которого будет выполнено чтение данных. </param>
    /// <param name="textEncoding">
    /// Определяет кодировку текста, используемую для чтения файла. По-умолчанию используется кодировка UTF-8.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="byteOrder">
    /// Порядок байтов, используемый для декодирования целых чисел при чтении из потока.
    /// Значение по умолчанию: LittleEndian. </param>
    /// <param name="lineSplitter">
    /// Строка, используемая в качестве разделителя строки в файле.
    /// Значение по умолчанию: Неопределено. </param>
    /// <param name="convertibleSplitterOfLines">
    /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
    /// Значение по умолчанию: ВК + ПС. </param>
    [ScriptConstructor(Name = "На основании имени файла")]
    public static IRuntimeContextInstance Constructor(string fileName, IValue textEncoding = null, IValue byteOrder = null, string lineSplitter = null, string convertibleSplitterOfLines = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Создает объект чтения из данного потока.
    /// После завершения работы с объектом ЧтениеДанных до того, как будет закрыт поток, переданный в конструктор, объект следует закрыть с помощью метода Закрыть или НачатьЗакрытие.
    /// </summary>
    ///
    /// <param name="stream">
    /// Поток, из которого будет производиться чтение данных. Типы: Поток (Stream), ПотокВПамяти (MemoryStream), ФайловыйПоток (FileStream) </param>
    /// <param name="textEncoding">
    /// Определяет кодировку текста, используемую для чтения данных. По-умолчанию используется кодировка UTF-8.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// Значение по умолчанию: UTF8. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="byteOrder">
    /// Порядок байтов, используемый для декодирования целых чисел при чтении из потока.
    /// Значение по умолчанию: LittleEndian. </param>
    /// <param name="lineSplitter">
    /// Определяет строку, разделяющую строки в потоке.
    /// Значение по умолчанию: Неопределено. </param>
    /// <param name="convertibleSplitterOfLines">
    /// Определяет разделение строк в файле для конвертации в стандартный перевод строк ПС.
    /// Значение по умолчанию: ВК + ПС. </param>
    [ScriptConstructor(Name = "На основании потока")]
    public static IRuntimeContextInstance Constructor1(IValue stream, IValue textEncoding = null, IValue byteOrder = null, string lineSplitter = null, string convertibleSplitterOfLines = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Кодировка текста, используемая по-умолчанию для данного экземпляра ЧтениеДанных.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// </summary>
    [ContextProperty("КодировкаТекста", "TextEncoding")]
    public ByteOrderEnum TextEncoding { get; set; }
    
    /// <summary>
    /// 
    /// Конвертируемый разделитель строк. Этот параметр влияет на поведение метода ПрочитатьСимволы.
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
    public ByteOrderEnum ByteOrder { get; set; }
    
    /// <summary>
    /// 
    /// Разделитель строк по-умолчанию. Это свойство влияет на поведение метода ПрочитатьСтроку.
    /// </summary>
    /// <value>Строка (String)</value>
    [ContextProperty("РазделительСтрок", "LineSplitter")]
    public string LineSplitter { get; set; }
    
    /// <summary>
    /// 
    /// Содержит признак того, что во входном потоке больше нет данных для чтения. Изначально устанавливается в значение Ложь. Если при очередном чтении было прочитано меньше данных, чем было запрошено, принимает значение Истина.
    /// </summary>
    /// <value>Булево (Boolean)</value>
    [ContextProperty("ЧтениеЗавершено", "ReadCompleted")]
    public bool ReadCompleted { get; private set; }
    
    /// <summary>
    /// 
    /// Вызов данного метода завершает работу с текущим объектом. Если объект является владельцем вложенного потока, поток также закрывается.
    /// </summary>
    ///
    [ContextMethod("Закрыть", "Close")]
    public void Close()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Получает исходный поток, из которого выполняется чтение данных.
    /// </summary>
    ///
    ///
    /// <returns name="Stream">
    /// </returns>
    ///
    [ContextMethod("ИсходныйПоток", "SourceStream")]
    public IValue SourceStream()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Пропускает при чтении указанное количество байтов в потоке.
    /// </summary>
    ///
    /// <param name="number">
    /// Количество байтов, которые требуется пропустить. </param>
    ///
    /// <returns name="Number">
    /// </returns>
    ///
    [ContextMethod("Пропустить", "Skip")]
    public int Skip(int number)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Пропускает при чтении двоичные данные до указанного разделителя.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Пропуск до двоичного маркера
    /// </remarks>
    ///
    /// <param name="marker">
    /// Маркер, до которого требуется пропустить данные. </param>
    ///
    /// <returns name="Number"/>
    ///
    [ContextMethod("ПропуститьДо", "SkipTo")]
    public int SkipTo(IValue marker)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Пропускает при чтении двоичные данные до указанного разделителя.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Пропуск до строкового маркера
    /// </remarks>
    ///
    /// <param name="marker">
    /// Маркер, до которого требуется пропустить данные. </param>
    /// <param name="encoding">
    /// Кодировка текста.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    /// <returns name="Number"/>
    ///
    [ContextMethod("ПропуститьДо", "SkipTo")]
    public int SkipTo(string marker, IValue encoding = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Пропускает при чтении двоичные данные до указанного разделителя.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Пропуск до смешанного набора маркеров
    /// </remarks>
    ///
    /// <param name="markers">
    /// Массив маркеров, до которых надо пропустить данные. Элементы массива могут иметь тип БуферДвоичныхДанных или Строка.
    /// Данные пропускаются до первого встреченного маркера наибольшей возможной длины. </param>
    /// <param name="encoding">
    /// Кодировка текста.
    /// 
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    [ContextMethod("ПропуститьДо", "SkipTo")]
    public int SkipTo(IValue markers, IValue encoding = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Выполняет чтение двоичных данных заданного размера из потока. Если размер не указан, будут прочитаны все данные до конца потока.
    /// </summary>
    ///
    /// <param name="number">
    /// Количество байтов, которые требуется прочитать. Если не задано, то выполняется чтение всех данных до конца потока.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="ReadDataResult">
    /// Содержит описание результата чтения данных из потока.</returns>
    ///
    [ContextMethod("Прочитать", "Read")]
    public IValue Read(int number = 0)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Выполняет чтение одного байта из потока.
    /// </summary>
    ///
    /// <returns name="Number"/>
    /// 
    [ContextMethod("ПрочитатьБайт", "ReadByte")]
    public int ReadByte()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Прочитать байты из потока в БуферДвоичныхДанных.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Чтение в новый буфер
    /// </remarks>
    ///
    /// <param name="number">
    /// Количество байтов, которые требуется прочитать. Если не задано, то выполняется чтение всех байтов до конца потока.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="BinaryDataBuffer"/>
    ///
    [ContextMethod("ПрочитатьВБуферДвоичныхДанных", "ReadIntoBinaryDataBuffer")]
    public IValue ReadIntoBinaryDataBuffer(int number = 0)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Прочитать байты из потока в БуферДвоичныхДанных.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Чтение в существующий буфер
    /// </remarks>
    ///
    /// <param name="buffer">
    /// Буфер двоичных данных, в который требуется поместить прочитанные байты. </param>
    /// <param name="positionInBuffer">
    /// Позиция в буфере, начиная с которой требуется записать прочитанные данные. </param>
    /// <param name="number">
    /// Количество байтов, которые требуется прочитать. </param>
    ///
    /// <returns name="BinaryDataBuffer"/>
    ///
    [ContextMethod("ПрочитатьВБуферДвоичныхДанных", "ReadIntoBinaryDataBuffer")]
    public IValue ReadIntoBinaryDataBuffer(IValue buffer, int positionInBuffer, int number)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Выполняет чтение двоичных данных до указанного маркера.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Чтение до двоичного маркера
    /// </remarks>
    ///
    /// <param name="marker">
    /// Маркер, до которого выполняется чтение данных. </param>
    ///
    /// <returns name="ReadDataResult">
    /// Содержит описание результата чтения данных из потока.</returns>
    ///
    [ContextMethod("ПрочитатьДо", "ReadTo")]
    public IValue ReadTo(IValue marker)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Выполняет чтение двоичных данных до указанного маркера.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Чтение до строкового маркера
    /// </remarks>
    ///
    /// <param name="marker">
    /// Маркер, до которого выполняется чтение. </param>
    /// <param name="encoding">
    /// Кодировка текста для определения строковых маркеров в двоичном потоке. 
    /// Если параметр не установлен, используется кодировка, указанная для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    /// <returns name="ReadDataResult">
    /// Содержит описание результата чтения данных из потока.</returns>
    ///
    [ContextMethod("ПрочитатьДо", "ReadTo")]
    public IValue ReadTo(string marker, IValue encoding = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Выполняет чтение двоичных данных до указанного маркера.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// Чтение до смешанного набора маркеров
    /// </remarks>
    ///
    /// <param name="markers">
    /// Массив, содержащий маркеры. Элементы массива могут иметь тип БуферДвоичныхДанных или Строка. </param>
    /// <param name="encoding">
    /// Кодировка текста для определения строковых маркеров в двоичном потоке. 
    /// Если параметр не установлен, используется кодировка, указанная для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    /// <returns name="ReadDataResult">
    /// Содержит описание результата чтения данных из потока.</returns>
    ///
    [ContextMethod("ПрочитатьДо", "ReadTo")]
    public IValue ReadTo(IValue markers, IValue encoding = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Выполняет чтение заданного количества символов из потока в виде строки. Если количество символов не задано, выполняется чтение всего остатка потока.
    /// </summary>
    ///
    /// <param name="count">
    /// Количество символов, которые требуется прочитать. Если не установлено, то будут прочитаны все символы до конца потока.
    /// Значение по умолчанию: Неопределено. </param>
    /// <param name="encoding">
    /// Определяет кодировку текста. Если не установлена, используется кодировка, заданная для данного объекта ЧтениеДанных.
    /// Кодировка может быть задана как в виде значения перечисления КодировкаТекста, так и в виде строки с указанием названия кодировки.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    /// <returns name="String"/>
    ///
    [ContextMethod("ПрочитатьСимволы", "ReadChars")]
    public string ReadChars(int count = 0, IValue encoding = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Выполняет чтение строки текста до разделителя строки. Если разделитель не найден, чтение выполняется до конца потока.
    /// Если разделитель строки не задан явно, используется разделитель строки, указанный для данного экземпляра объекта ЧтениеДанных.
    /// </summary>
    ///
    /// <param name="encoding">
    /// Кодировка текста. Если не установлена, используется кодировка, заданная для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    /// <param name="lineSplitter">
    /// Указывает строку, являющуюся разделителем строк в читаемых данных. Если параметр не указан, используется разделитель строк, указанный для текущего экземпляра объекта ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="String"/>
    ///
    [ContextMethod("ПрочитатьСтроку", "ReadLine")]
    public string ReadLine(IValue encoding = null, string lineSplitter = null)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Считывает 16-битное целое число из потока.
    /// </summary>
    ///
    /// <param name="byteOrder">
    /// Порядок байтов, используемый при чтении числа.
    /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="Number"/>
    ///
    [ContextMethod("ПрочитатьЦелое16", "ReadInt16")]
    public int ReadInt16(IValue byteOrder = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Прочитать 32-битное целое число из потока.
    /// </summary>
    ///
    /// <param name="byteOrder">
    /// Порядок байтов, используемый при чтении числа.
    /// Если не задан, используется порядок, определенный для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="Number">
    /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>
    ///
    [ContextMethod("ПрочитатьЦелое32", "ReadInt32")]
    public int ReadInt32(IValue byteOrder = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Считывает 64-битное целое число из потока.
    /// </summary>
    ///
    /// <param name="byteOrder">
    /// Устанавливает порядок байтов, используя который число будет прочитано. Если порядок байтов не задан, то используется порядок байтов, определенный для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. </param>
    ///
    /// <returns name="Number">
    /// Числовым типом может быть представлено любое десятичное число. Над данными числового типа определены основные арифметические операции: сложение, вычитание, умножение и деление. Максимально допустимая разрядность числа 38 знаков.</returns>
    ///
    [ContextMethod("ПрочитатьЦелое64", "ReadInt64")]
    public int ReadInt64(IValue byteOrder = null)
    {
        throw new NotImplementedException();
    }


    /// <summary>
    /// 
    /// Разделяет остаток данных по заданным разделителям.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// По двоичному разделителю
    /// </remarks>
    ///
    /// <param name="separator">
    /// Двоичный разделитель данных. </param>
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
    /// Разделяет остаток данных по заданным разделителям.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// По строковому разделителю
    /// </remarks>
    ///
    /// <param name="separator">
    /// Разделитель данных в виде строки. </param>
    /// <param name="encoding">
    /// Кодировка текста. Если не задана, то используется кодировка, заданная для текущего экземпляра объекта ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    /// <returns name="Array"/>
    ///
    [ContextMethod("Разделить", "Split")]
    public IValue Split(string separator, IValue encoding = null)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// Разделяет остаток данных по заданным разделителям.
    /// </summary>
    ///
    /// <remarks>
    /// 
    /// По набору смешанных разделителей
    /// </remarks>
    ///
    /// <param name="separators">
    /// Массив разделителей может содержать элементы типа БуферДвоичныхДанных или Строка. </param>
    /// <param name="encoding">
    /// Кодировка текста. Если не установлена, используется кодировка, заданная для текущего экземпляра ЧтениеДанных.
    /// Значение по умолчанию: Неопределено. Типы: КодировкаТекста (TextEncoding), Строка (String) </param>
    ///
    /// <returns name="Array"/>
    ///
    [ContextMethod("Разделить", "Split")]
    public IValue Split(IValue separators, IValue encoding = null)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// Разделяет остаток данных на части заданного размера. Размер части указывается в байтах.
    /// </summary>
    ///
    /// <param name="partSizw">
    /// Размер части данных в байтах. </param>
    ///
    /// <returns name="Array"/>
    ///
    [ContextMethod("РазделитьНаЧастиПо", "SplitInPartsOf")]
    public IValue SplitInPartsOf(int partSizw)
    {
        throw new NotImplementedException();
    }

}
