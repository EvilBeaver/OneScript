/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Text;
using OneScript.StandardLibrary.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OneScript.StandardLibrary.Json
{
    /// <summary>
    /// Предоставляет методы для извлечения данных из JSON по запросу JSON-path
    /// </summary>
    [ContextClass("ИзвлечениеДанныхJSON", "JSONDataExtractor")]
    public class JSONDataExtractor : AutoContext<JSONDataExtractor>
    {
        private JToken _jsonData;

        /// <summary>
        /// Устанавливает строку JSON для последующей обработки
        /// </summary>
        /// <param name="JSONString">Строка. Строка JSON.</param>
        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(string JSONString)
        {

            _jsonData = JToken.Parse(JSONString);

        }

        /// <summary>
        /// Читает файл, содержащий данные JSON для последующей обработки
        /// </summary>
        /// <param name="JSONFileName">Строка. Путь к файлу.</param>
        /// <param name="encoding">КодировкаТекста. кодировка файла.</param>
        [ContextMethod("ОткрытьФайл", "OpenFile")]
        public void OpenFile(string JSONFileName, IValue encoding = null)
        {

            StreamReader _fileReader;

            try
            {
                if (encoding == null)
                    _fileReader = FileOpener.OpenReader(JSONFileName, Encoding.Default);
                else
                    _fileReader = FileOpener.OpenReader(JSONFileName, TextEncodingEnum.GetEncoding(encoding));
            }
            catch (Exception e)
            {
                throw new RuntimeException(e.Message, e);
            }

            _jsonData = JToken.Parse(_fileReader.ReadToEnd());

            _fileReader.Close();

        }

        /// <summary>
        /// Читает данные из потока JSON для последующей обработки
        /// </summary>
        /// <param name="JSONStream">Поток. поток с данными JSON.</param>
        /// <param name="encoding">КодировкаТекста. кодировка файла.</param>
        [ContextMethod("ОткрытьПоток", "OpenStream")]
        public void OpenStream(IValue JSONStream, IValue encoding = null)
        {

            string _JSONString = "{}";
            Encoding _encoding;

            if (encoding == null)
                _encoding = Encoding.Default;
            else
                _encoding = TextEncodingEnum.GetEncoding(encoding);

            using (Stream _underlyingStream = ((IStreamWrapper)JSONStream).GetUnderlyingStream())
            {
                byte[] buffer = new byte[1000];
                StringBuilder builder = new StringBuilder();
                int read = -1;

                while (true)
                {
                    AutoResetEvent gotInput = new AutoResetEvent(false);
                    Thread inputThread = new Thread(() =>
                    {
                        try
                        {
                            read = _underlyingStream.Read(buffer, 0, buffer.Length);
                            gotInput.Set();
                        }
                        catch (ThreadAbortException)
                        {
                            Thread.ResetAbort();
                        }
                    })
                    {
                        IsBackground = true
                    };

                    inputThread.Start();

                    // Timeout expired?
                    if (!gotInput.WaitOne(100))
                    {
                        inputThread.Abort();
                        break;
                    }

                    // End of stream?
                    if (read == 0)
                    {
                        _JSONString = builder.ToString();
                        break;
                    }

                    // Got data
                    builder.Append(_encoding.GetString(buffer, 0, read));
                }
            }

            string _BOMMarkUTF8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

            if (_JSONString.StartsWith(_BOMMarkUTF8, StringComparison.Ordinal))
                _JSONString = _JSONString.Remove(0, _BOMMarkUTF8.Length);

            _jsonData = JToken.Parse(_JSONString.Trim());

        }

        /// <summary>
        /// Выполняет выборку из JSON по указанному JSON-path
        /// </summary>
        /// <param name="path">Строка. JSON-path.</param>
        /// <param name="extractSingleValue">Булево. Если результирующий массив содержит единственное значение, то:
        /// Истина -  будет возвращено значение;
        /// Ложь - будет возвращен массив.</param>
        /// <param name="getObjectAsJSON">Булево. Истина - объекты будут возвращены в виде строки JSON;
        /// Ложь - Объекты будут возвращены в виде соответствия.</param>
        /// <returns>Строка - Выбранные данные</returns>
        [ContextMethod("Выбрать", "Select")]
        public IValue Select(string path, bool extractSingleValue = true, bool getObjectAsJSON = true)
        {
            IValue result;

            AggregateFuncEnum aggregateFunc = GetAggregateFunc(path, out path);

            List<JToken> parseResult = _jsonData.SelectTokens(path).ToList();

            if (parseResult.Count() == 0)
            {
                result = ValueFactory.Create();
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);

                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartArray();
                    foreach (JToken token in parseResult)
                    {
                        writer.WriteToken(token.CreateReader(), true);
                    }
                    writer.WriteEndArray();
                }
                result = JSONToDataStructure(sb.ToString());
            }

            if (aggregateFunc != AggregateFuncEnum.none)
                result = CalculateAggregateFunc(result, aggregateFunc);

            if (result is ArrayImpl && ((ArrayImpl)result.AsObject()).Count() == 1 && extractSingleValue)
            {
                result = ((ArrayImpl)result.AsObject()).Get(0);
            }

            if ((result is ArrayImpl || result is MapImpl) && getObjectAsJSON)
            {
                result = ValueFactory.Create(DataStructureToJSON(result));
            }

            return result;
        }

        #region JSON conversion

        /// <summary>
        /// Преобразует строку JSON в соответствие или массив
        /// </summary>
        /// <param name="JSONString">Строка. Строка JSON.</param>
        /// <returns>Соответствие, Массив - Результат преобразования строки JSON в соответствие или массив</returns>
        private IValue JSONToDataStructure(string JSONString)
        {
            JSONReader reader = new JSONReader();
            reader.SetString(JSONString);

            return ((GlobalJsonFunctions)GlobalJsonFunctions.CreateInstance()).ReadJSONInMap(reader);
        }

        /// <summary>
        /// Преобразует соответствие или массив в строку JSON
        /// </summary>
        /// <param name="inputStruct">Соответствие, Массив. Соответствие или массив для преобразования в строку JSON.</param>
        /// <returns>Строка - Результат преобразования соответствия или массива в строку JSON</returns>
        private string DataStructureToJSON(IValue inputStruct)
        {
            JSONWriter writer = new JSONWriter();
            writer.SetString();

            ((GlobalJsonFunctions)GlobalJsonFunctions.CreateInstance()).WriteJSON(writer, inputStruct);

            return writer.Close();
        }

        #endregion

        #region Aggregate calculation

        /// <summary>
        /// Вычисляет агрегатную функцию над переданными данными
        /// </summary>
        /// <param name="sourceData">Соответствие, Массив. Соответствие или массив для вычисления агрегатной функции.</param>
        /// <param name="aggregateFunc">Агрегатнаная функция.</param>
        /// <returns>Строка - Результат вычисления функции</returns>
        private IValue CalculateAggregateFunc(IValue sourceData, AggregateFuncEnum aggregateFunc)
        {

            IValue result;

            if (aggregateFunc == AggregateFuncEnum.length)
                result = CalculateLength(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.sum)
                result = CalculateSum(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.avg)
                result = CalculateAvg(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.min)
                result = CalculateMin(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.max)
                result = CalculateMax(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.first)
                result = CalculateFirst(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.last)
                result = CalculateLast(sourceData);
            else if (aggregateFunc == AggregateFuncEnum.keys)
                result = CalculateKeys(sourceData);
            else
                result = sourceData;

            return result;
        }

        /// <summary>
        /// Вычисляет размер переданного массива или соответствия
        /// </summary>
        /// <param name="sourceData">Соответствие, Массив. Соответствие или массив для обработки.</param>
        /// <returns>Число - Размер переданного массива или соответствия</returns>
        private IValue CalculateLength(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (sourceData is ArrayImpl)
                result = ValueFactory.Create(((ArrayImpl)sourceData).Count());
            else if (sourceData is MapImpl)
                result = ValueFactory.Create(((MapImpl)sourceData).Count());

            return result;
        }

        /// <summary>
        /// Вычисляет сумму чисел в переданном массиве
        /// </summary>
        /// <param name="sourceData">Массив. Соответствие или массив для обработки.</param>
        /// <returns>Число - Сумма чисел в переданном массиве</returns>
        private IValue CalculateSum(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (((ArrayImpl)sourceData).Count() == 0)
                return result;

            decimal fullSum = 0;

            foreach (IValue value in (ArrayImpl)sourceData)
                fullSum += value.AsNumber();

            result = ValueFactory.Create(fullSum);

            return result;
        }

        /// <summary>
        /// Вычисляет среднее значение в переданном массиве
        /// </summary>
        /// <param name="sourceData">Массив. Соответствие или массив для обработки.</param>
        /// <returns>Число - Среднее значение в переданном массиве</returns>
        private IValue CalculateAvg(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (!(sourceData is ArrayImpl))
                return result;

            if (((ArrayImpl)sourceData).Count() == 0)
                return result;

            decimal fullSum = 0;

            foreach (IValue value in (ArrayImpl)sourceData)
                fullSum += value.AsNumber();

            result = ValueFactory.Create(fullSum / ((ArrayImpl)sourceData).Count());

            return result;
        }

        /// <summary>
        /// Вычисляет минимально значение в переданном массиве
        /// </summary>
        /// <param name="sourceData">Массив. Соответствие или массив для обработки.</param>
        /// <returns>Число - Минимальное значение в переданном массиве</returns>
        private IValue CalculateMin(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (!(sourceData is ArrayImpl))
                return result;

            if (((ArrayImpl)sourceData).Count() == 0)
                return result;

            decimal numericResult = ((ArrayImpl)sourceData).Get(0).AsNumber();

            foreach (IValue value in (ArrayImpl)sourceData)
                numericResult = value.AsNumber() < numericResult ? value.AsNumber() : numericResult;

            result = ValueFactory.Create(numericResult);

            return result;
        }

        /// <summary>
        /// Вычисляет максимальное значение в переданном массиве
        /// </summary>
        /// <param name="sourceData">Массив. Соответствие или массив для обработки.</param>
        /// <returns>Число - Максимальное значение в переданном массиве</returns>
        private IValue CalculateMax(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (!(sourceData is ArrayImpl))
                return result;

            if (((ArrayImpl)sourceData).Count() == 0)
                return result;

            decimal numericResult = ((ArrayImpl)sourceData).Get(0).AsNumber();

            foreach (IValue value in (ArrayImpl)sourceData)
                numericResult = value.AsNumber() > numericResult ? value.AsNumber() : numericResult;

            result = ValueFactory.Create(numericResult);

            return result;
        }

        /// <summary>
        /// Получает первое значение из переданного массива
        /// </summary>
        /// <param name="sourceData">Массив. Массив для обработки.</param>
        /// <returns>Произвольный - Первое значение из переданного массива</returns>
        private IValue CalculateFirst(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (!(sourceData is ArrayImpl))
                return result;

            if (((ArrayImpl)sourceData).Count() == 0)
                return result;

            result = ((ArrayImpl)sourceData).Get(0);

            return result;
        }

        /// <summary>
        /// Получает последнее значение из переданного массива
        /// </summary>
        /// <param name="sourceData">Массив. Массив для обработки.</param>
        /// <returns>Произвольный - Последнее значение из переданного массива</returns>
        private IValue CalculateLast(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (!(sourceData is ArrayImpl))
                return result;

            if (((ArrayImpl)sourceData).Count() == 0)
                return result;

            int lastIndex = ((ArrayImpl)sourceData).UpperBound();
            result = ((ArrayImpl)sourceData).Get(lastIndex);

            return result;
        }

        /// <summary>
        /// Получает массив ключей из переданного соответствия или массив индексов из переданного массива
        /// если переданный массив содержит единственное соответствие, то возвращаются ключи соответствия
        /// </summary>
        /// <param name="sourceData">Соответствие, Массив. Соответствие или массив для обработки.</param>
        /// <returns>Массив - Ключи соответствия или индексы массива</returns>
        private IValue CalculateKeys(IValue sourceData)
        {

            IValue result = ValueFactory.Create();

            if (sourceData is ArrayImpl && ((ArrayImpl)sourceData).Count() == 1)
                sourceData = ((ArrayImpl)sourceData).Get(0);

            if (!(sourceData is MapImpl))
                return result;

            ArrayImpl keyArray = new ArrayImpl();
            if (sourceData is MapImpl)
                foreach (KeyAndValueImpl KeyValue in (MapImpl)sourceData)
                    keyArray.Add(KeyValue.Key);
            else if (sourceData is ArrayImpl)
                for (int i = 0; i < ((ArrayImpl)sourceData).Count(); i++)
                    keyArray.Add(ValueFactory.Create(i));

            return keyArray;
        }

        /// <summary>
        /// Доступные агрегатные функции
        /// </summary>
        private enum AggregateFuncEnum
        {
            none,
            length,
            sum,
            avg,
            min,
            max,
            first,
            last,
            keys
        }

        /// <summary>
        /// Получает используемую агрегатную функцию из переданного пути JSON path
        /// </summary>
        /// <param name="path">Строка. исходный путь JSON-path.</param>
        /// <param name="mainPath">Строка. путь JSON-path без агрегатной функции.</param>
        /// <returns>AggregateFuncEnum - используемуя агрегатная функция</returns>
        private AggregateFuncEnum GetAggregateFunc(string path, out string mainPath)
        {
            Dictionary<string, AggregateFuncEnum> aggregateFuncs = new Dictionary<string, AggregateFuncEnum>();
            aggregateFuncs.Add("count", AggregateFuncEnum.length);
            aggregateFuncs.Add("length", AggregateFuncEnum.length);
            aggregateFuncs.Add("sum", AggregateFuncEnum.sum);
            aggregateFuncs.Add("avg", AggregateFuncEnum.avg);
            aggregateFuncs.Add("min", AggregateFuncEnum.min);
            aggregateFuncs.Add("max", AggregateFuncEnum.max);
            aggregateFuncs.Add("first", AggregateFuncEnum.first);
            aggregateFuncs.Add("last", AggregateFuncEnum.last);
            aggregateFuncs.Add("keys", AggregateFuncEnum.keys);

            AggregateFuncEnum result = AggregateFuncEnum.none;
            mainPath = path;

            string[] parts = path.Split('.');

            if (parts.Length == 0)
                return result;

            string aggregateFunc = parts[parts.GetUpperBound(0)].Trim();

            if (!aggregateFunc.Trim().EndsWith("()"))
                return result;

            aggregateFunc = aggregateFunc.Substring(0, aggregateFunc.Length - 2);

            if (aggregateFuncs.TryGetValue(aggregateFunc.ToLower(), out result))
            {
                mainPath = String.Join(".", parts, 0, parts.Length - 1);
                return result;
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Создает ИзвлечениеДанныхJSON
        /// </summary>
        /// <returns>ИзвлечениеДанныхJSON</returns>
        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new JSONDataExtractor();
        }

    }
}
