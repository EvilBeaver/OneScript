/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OneScript.Core;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    [ContextClass("ТекстовыйДокумент", "TextDocument")]
    public class TextDocumentContext : AutoContext<TextDocumentContext>
    {
        private List<string> _lines;

        private string _lineSeparator;

        public TextDocumentContext()
        {
            LineSeparator = "\n";
            _lines = new List<string>();
        }

        #region Свойства

        /// <summary>
        /// В 1С:Предприятие определяет возможность печати документа.
        /// В 1Script свойство не используется и его значение игнорируется
        /// </summary>
        [ContextProperty("Вывод", "Output")]
        public IValue Output
        {
            get { return ValueFactory.Create(); }
            set {  }
        }

        /// <summary>
        /// Содержит полное имя файла с которым соединен ТекстовыйДокумент
        /// </summary>
        /// <returns>Строка</returns>
        [ContextProperty("ИспользуемоеИмяФайла", "UsedFileName")]
        public string UsedFileName { get; private set; }

        /// <summary>
        /// Не используется. Реализован для совместимости API с 1С:Предприятие
        /// </summary>
        /// <returns>Строка</returns>
        [ContextProperty("КодЯзыкаМакета", "TemplateLanguageCode")]
        public string TemplateLanguageCode { get; set; }

        /// <summary>
        /// Не используется. Реализован для совместимости API с 1С:Предприятие
        /// </summary>
        /// <returns>Неопределено</returns>
        [ContextProperty("Параметры", "Parameters")]
        public IValue Parameters
        {
            get { return ValueFactory.Create(); }
            set { }
        }

        [ContextProperty("РазделительСтрок", "LineSeparator")]
        public string LineSeparator
        {
            get { return _lineSeparator; }
            set
            {
                if (value == "\n" || value == "\r" || value == "\r\n")
                    _lineSeparator = value;
                else
                    throw RuntimeException.InvalidArgumentValue();
            }
        }

        /// <summary>
        /// Не используется. Реализован для совместимости API с 1С:Предприятие
        /// </summary>
        [ContextProperty("ТолькоПросмотр", "ReadOnly")]
        public bool ReadOnly { get; set; }

        #endregion

        private List<string> ParseInputString(string input)
        {
            var output = new List<string>();
            if (input == String.Empty)
                output.Add("");
            else
            {
                using (var reader = new StringReader(input))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        output.Add(line);
                    }
                }
            }

            return output;
        }

        private void Reset()
        {
            _lines.Clear();
        }

        /// <summary>
        /// Вставляет строку в документ
        /// </summary>
        /// <param name="position">Позиция вставки</param>
        /// <param name="line">Вставляемая строка</param>
        [ContextMethod("ВставитьСтроку", "InsertLine")]
        public void InsertLine(int position, string line)
        {
            var localLines = ParseInputString(line);

            int insertionPos = position - 1;
            if (insertionPos < 0)
                insertionPos = 0;
            else if (insertionPos > _lines.Count)
                insertionPos = _lines.Count;

            _lines.InsertRange(insertionPos, localLines);
        }

        /// <summary>
        /// Добавляет строку в конец текстового документа
        /// </summary>
        /// <param name="line">Добавляемая строка</param>
        [ContextMethod("ДобавитьСтроку", "AddLine")]
        public void AddLine(string line)
        {
            var localLines = ParseInputString(line);
            _lines.AddRange(localLines);
        }

        [ContextMethod("КоличествоСтрок", "LineCount")]
        public int LineCount()
        {
            return _lines.Count;
        }

        /// <summary>
        /// Получает текст, находящийся в текстовом документе
        /// </summary>
        /// <returns></returns>
        [ContextMethod("ПолучитьТекст", "GetText")]
        public string GetText()
        {
            var builder = new StringBuilder();
            foreach (var line in _lines)
            {
                builder.AppendFormat("{0}\n", line);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Получает строку по номеру
        /// </summary>
        /// <param name="lineNumber">Номер строки в тексте</param>
        /// <returns>Строка</returns>
        [ContextMethod("ПолучитьСтроку", "GetLine")]
        public string GetLine(int lineNumber)
        {
            if (lineNumber < 1 || lineNumber > _lines.Count)
                return "";

            return _lines[lineNumber - 1];
        }

        /// <summary>
        /// Заменяет содержимое строки по номеру
        /// </summary>
        /// <param name="number">Номер заменяемой строки</param>
        /// <param name="newLine">Новое значение строки</param>
        [ContextMethod("ЗаменитьСтроку", "ReplaceLine")]
        public void ReplaceLine(int number, string newLine)
        {
            if (number > _lines.Count)
                return;

            if (number < 1)
                throw RuntimeException.InvalidArgumentValue();

            var newLines = ParseInputString(newLine);
            if (newLines.Count == 1)
                _lines[number - 1] = newLines[0];
            else
            {
                _lines[number - 1] = newLines[0];
                _lines.InsertRange(number, newLines.Skip(1));
            }
        }

        /// <summary>
        /// Очищает содержимое документа
        /// </summary>
        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            Reset();
        }

        /// <summary>
        /// Устанавливает содержимое текстового документа
        /// </summary>
        /// <param name="newText">Новый текст документа</param>
        [ContextMethod("УстановитьТекст", "SetText")]
        public void SetText(string newText)
        {
            Reset();
            _lines.AddRange(ParseInputString(newText));
        }

        /// <summary>
        /// Удаляет строку по номеру
        /// </summary>
        /// <param name="lineNumber">Номер удаляемой строки</param>
        [ContextMethod("УдалитьСтроку", "DeleteLine")]
        public void DeleteLine(int lineNumber)
        {
            if (lineNumber < 1 || lineNumber > _lines.Count)
                return;

            _lines.RemoveAt(lineNumber - 1);
        }

        /// <summary>
        /// Читает содержимое из файла
        /// </summary>
        /// <param name="path">Имя файла</param>
        /// <param name="encoding">Кодировка</param>
        /// <param name="lineSeparator">Разделитель строк в файле. FIXME: На данный момент параметр игнорируется, при чтении применяется разделитель для текущей ОС.</param>
        [ContextMethod("Прочитать", "Read")]
        public void Read(string path, IValue encoding = null, string lineSeparator = null)
        {
            var newContent = new List<string>();

            using (var reader = GetDefaultReader(path, encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    newContent.Add(line);
                }
            }

            Reset();
            _lines = newContent;
            UsedFileName = Path.GetFullPath(path);
        }

        /// <summary>
        /// Записывает содержимое документа в файл
        /// </summary>
        /// <param name="path">Путь файла</param>
        /// <param name="encoding">Кодировка файла</param>
        /// <param name="lineSeparator">Разделитель строк. По умолчанию - ВК+ПС</param>
        [ContextMethod("Записать", "Write")]
        public void Write(string path, IValue encoding = null, string lineSeparator = null)
        {
            using (var writer = GetDefaultWriter(path, encoding))
            {
                if (lineSeparator == null)
                    lineSeparator = "\r\n";
                else if (lineSeparator != "\n" && lineSeparator != "\r" && lineSeparator != "\r\n")
                    throw RuntimeException.InvalidArgumentValue();

                foreach (var line in _lines)
                {
                    writer.Write(line);
                    writer.Write(lineSeparator);
                }
            }

            UsedFileName = Path.GetFullPath(path);
        }

        private StreamReader GetDefaultReader(string path, IValue encoding)
        {
            StreamReader reader;
            if (encoding == null)
                reader = ScriptEngine.Environment.FileOpener.OpenReader(path);
            else
                reader = ScriptEngine.Environment.FileOpener.OpenReader(path, TextEncodingEnum.GetEncoding(encoding));

            return reader;
        }

        private StreamWriter GetDefaultWriter(string path, IValue encoding)
        {
            StreamWriter writer;
            if (encoding == null)
                writer = ScriptEngine.Environment.FileOpener.OpenWriter(path, new UTF8Encoding(true));
            else
                writer = ScriptEngine.Environment.FileOpener.OpenWriter(path, TextEncodingEnum.GetEncoding(encoding));

            return writer;
        }

        [ScriptConstructor]
        public static TextDocumentContext Create()
        {
            return new TextDocumentContext();
        }
    }
}