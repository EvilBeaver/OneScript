﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Глобальный контекст. Операции с двоичными данными.
    /// </summary>
    [GlobalContext(Category = "Процедуры и функции работы с двоичными данными")]
    public sealed class GlobalBinaryData : GlobalContextBase<GlobalBinaryData>
    {
        public static IAttachableContext CreateInstance()
        {
            return new GlobalBinaryData();
        }

        /// <summary>
        /// Объединяет несколько объектов типа ДвоичныеДанные в один.
        /// </summary>
        /// <param name="array">Массив объектов типа ДвоичныеДанные.</param>
        /// <returns>Тип: ДвоичныеДанные.</returns>
        [ContextMethod("СоединитьДвоичныеДанные")]
        public BinaryDataContext  ConcatenateBinaryData(ArrayImpl array)
        {
            // Сделано на int т.к. BinaryContext.Size имеет тип int;

            using (var stream = new System.IO.MemoryStream())
            {

                foreach (var cbd in array)
                {
                    byte[] buffer = ((BinaryDataContext) cbd.AsObject()).Buffer;
                    stream.Write(buffer, 0, buffer.Length);
                }

                return new BinaryDataContext(stream.ToArray());
            }
        }

        /// <summary>
        /// Разделяет двоичные данные на части заданного размера. Размер задается в байтах.
        /// </summary>
        /// <param name="data">Объект типа ДвоичныеДанные.</param>
        /// <param name="size">Размер одной части данных.</param>
        /// <returns>Массив объектов типа ДвоичныеДанные.</returns>
        [ContextMethod("РазделитьДвоичныеДанные")]
        public ArrayImpl SplitBinaryData(BinaryDataContext data, int size)
        {
            // Сделано на int т.к. BinaryContext.Size имеет тип int;
            ArrayImpl array = new ArrayImpl();

            int readedBytes = 0;
            
            while (readedBytes < data.Buffer.Length)
            {
                int bytesToRead = size;
                if (bytesToRead > data.Buffer.Length - readedBytes)
                    bytesToRead = data.Buffer.Length - readedBytes;

                byte[] buffer = new byte[bytesToRead];
                Buffer.BlockCopy(data.Buffer, readedBytes, buffer, 0, bytesToRead);
                readedBytes += bytesToRead;
                array.Add(new BinaryDataContext(buffer));
            }

            return array;
        }

        /// <summary>
        /// Преобразует строку в значение типа ДвоичныеДанные с учетом кодировки текста.
        /// </summary>
        /// <param name="str">Строка, которую требуется преобразовать в ДвоичныеДанные.</param>
        /// <param name="encoding">Кодировка текста</param>
        /// <param name="addBOM">Определяет, будет ли добавлена метка порядка байт (BOM) кодировки текста в начало данных.</param>
        /// <returns>Тип: ДвоичныеДанные.</returns>
        [ContextMethod("ПолучитьДвоичныеДанныеИзСтроки")]
        public BinaryDataContext GetBinaryDataFromString(string str, IValue encoding = null, bool addBOM = false)
        {
            // Получаем кодировку
            // Из синтаксис помощника если кодировка не задана используем UTF8

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding, addBOM);

            return new BinaryDataContext(enc.GetBytes(str));
        }

        // ToDo: ПолучитьБуферДвоичныхДанныхИзСтроки 

        [ContextMethod("ПолучитьСтрокуИзДвоичныхДанных")]
        public string GetStringFromBinaryData(BinaryDataContext data, IValue encoding = null)
        {
            // Получаем кодировку
            // Из синтаксис помощника если кодировка не задана используем UTF8

            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);

            return enc.GetString(data.Buffer);
        }

        // ToDo: ПолучитьСтрокуИзБуфераДвоичныхДанных


        /// <summary>
        /// Преобразует строку формата Base64 в двоичные данные.
        /// </summary>
        /// <param name="str">Строка в формате Base64.</param>
        /// <returns>Тип: ДвоичныеДанные.</returns>
        [ContextMethod("ПолучитьДвоичныеДанныеИзBase64Строки")]
        public BinaryDataContext GetBinaryDataFromBase64String(string str)
        {
            return new BinaryDataContext(System.Convert.FromBase64String(str));
        }

        // ToDo: ПолучитьБуферДвоичныхДанныхИзBase64Строки

        [ContextMethod("ПолучитьBase64СтрокуИзДвоичныхДанных")]
        public string GetBase64StringFromBinaryData(BinaryDataContext data)
        {
            return System.Convert.ToBase64String(data.Buffer);
        }

        // ToDo: ПолучитьBase64СтрокуИзБуфераДвоичныхДанных

        // ToDo: ПолучитьДвоичныеДанныеИзBase64ДвоичныхДанных

        // ToDo: ПолучитьБуферДвоичныхДанныхИзBase64БуфераДвоичныхДанных

        // ToDo: ПолучитьBase64ДвоичныеДанныеИзДвоичныхДанных

        // ToDo: ПолучитьBase64БуферДвоичныхДанныхИзБуфераДвоичныхДанных

        /// <summary>
        /// Преобразует строку формата Base 16 (Hex) в двоичные данные.
        /// </summary>
        /// <param name="str">Строка в формате Base 16 (Hex).</param>
        /// <returns>Тип: ДвоичныеДанные.</returns>
        [ContextMethod("ПолучитьДвоичныеДанныеИзHexСтроки")]
        public BinaryDataContext GetBinaryDataFromHexString(string str)
        {
            int NumberChars = str.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(str.Substring(i, 2), 16);
            return new BinaryDataContext(bytes);
        }

        // ToDo: ПолучитьБуферДвоичныхДанныхИзHexСтроки

        // ToDo: ПолучитьHexСтрокуИзДвоичныхДанных

        // ToDo: ПолучитьHexСтрокуИзБуфераДвоичныхДанных

        // ToDo: ПолучитьДвоичныеДанныеИзHexДвоичныхДанных

        // ToDo: ПолучитьБуферДвоичныхДанныхИзHexБуфераДвоичныхДанных

        // ToDo: ПолучитьHexДвоичныеДанныеИзДвоичныхДанных

        // ToDo: ПолучитьHexБуферДвоичныхДанныхИзБуфераДвоичныхДанных

        /// <summary>
        /// Преобразует двоичные данные в буфер двоичных данных.
        /// </summary>
        /// <param name="data">Двоичные данные.</param>
        /// <returns>Тип: БуферДвоичныхДанных.</returns>
        [ContextMethod("ПолучитьБуферДвоичныхДанныхИзДвоичныхДанных")]
        public BinaryDataBuffer GetBinaryDataBufferFromBinaryData(BinaryDataContext data)
        {
            return new BinaryDataBuffer(data.Buffer);
        }

        /// <summary>
        /// Преобразует буфер двоичных данных в значение типа ДвоичныеДанные.
        /// </summary>
        /// <param name="buffer">Буфер двоичных данных.</param>
        /// <returns>Тип: ДвоичныеДанные.</returns>
        [ContextMethod("ПолучитьДвоичныеДанныеИзБуфераДвоичныхДанных")]
        public BinaryDataContext GetBinaryDataFromBinaryDataBuffer(BinaryDataBuffer buffer)
        {
            return new BinaryDataContext(buffer.Bytes);
        }

    }
}
