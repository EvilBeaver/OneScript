/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    // Преобразования двоичных данных в двоичные данные идут потоком: данные из временного файла
    // не читаются в память целиком, а результат больше лимита тоже уходит во временный файл
    public class BinaryDataStreamingTests
    {
        private const int LIMIT = 30;

        public static TheoryData<string> Base64Inputs => new TheoryData<string>
        {
            "", " ", "QQ==", "QUI=", "QUJD", "QUJDRA==", "QU JD\r\nRA==\r\n", "QQ=\n=", "Q Q = =",
            "QUJ", "Q===", "QQ=", "QQ==QQ==", "QUJDRA=a", "QUJD!", "QR==", "QUJD\vRA==", "﻿QUJD", "QUJD "
        };

        [Theory]
        [MemberData(nameof(Base64Inputs))]
        public void DecodesBase64LikeConvert(string base64)
        {
            byte[] expected;
            try
            {
                expected = Convert.FromBase64String(base64);
            }
            catch (FormatException)
            {
                expected = Array.Empty<byte>();
            }

            var result = Global(LIMIT).GetBinaryDataFromBase64BinaryData(InFile(Encoding.UTF8.GetBytes(base64)));

            ReadAll(result).Should().Equal(expected);
        }

        [Fact]
        public void LargeFileDataGoesThroughBase64AndHexInFiles()
        {
            // Больше порции чтения: четверки Base64 и пары hex-цифр попадают на границу порций
            var bytes = RandomBytes(200_000);
            var global = Global(LIMIT);

            var base64 = global.GetBase64BinaryDataFromBinaryData(InFile(bytes));
            base64.InMemory.Should().BeFalse();
            Encoding.ASCII.GetString(ReadAll(base64))
                .Should().Be(Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks));

            var decoded = global.GetBinaryDataFromBase64BinaryData(base64);
            decoded.InMemory.Should().BeFalse();
            ReadAll(decoded).Should().Equal(bytes);

            var hex = InFile(Encoding.ASCII.GetBytes(" " + Convert.ToHexString(bytes)));
            var fromHex = global.GetBinaryDataFromHexBinaryData(hex);
            fromHex.InMemory.Should().BeFalse();
            ReadAll(fromHex).Should().Equal(bytes);
        }

        [Fact]
        public void HexSkipsNonHexCharsAndDropsUnpairedDigit()
        {
            var hex = InFile(Encoding.ASCII.GetBytes("0a 1B\r\nfgFG-c"));

            ReadAll(Global(LIMIT).GetBinaryDataFromHexBinaryData(hex)).Should().Equal(0x0A, 0x1B, 0xFF);
        }

        [Fact]
        public void ConcatenatesMemoryAndFileDataIntoFile()
        {
            var first = new byte[] { 1, 2, 3 };
            var second = RandomBytes(40);
            var parts = new ArrayImpl();
            parts.Add(new BinaryDataContext(first));
            parts.Add(InFile(second));

            var result = Global(LIMIT).ConcatBinaryData(parts);

            result.InMemory.Should().BeFalse();
            ReadAll(result).Should().Equal(first.Concat(second));
        }

        [Fact]
        public void SplitsFileDataKeepingLargePartsInFiles()
        {
            var bytes = RandomBytes(100);

            var parts = Global(LIMIT).SplitBinaryData(InFile(bytes), 40)
                .Cast<BinaryDataContext>().ToArray();

            parts.Select(x => x.Size()).Should().Equal(40, 40, 20);
            parts.Select(x => x.InMemory).Should().Equal(false, false, true);
            parts.SelectMany(ReadAll).Should().Equal(bytes);
        }

        [Fact]
        public void ResultsStayInMemoryWithoutLimit()
        {
            var bytes = RandomBytes(1000);
            var global = Global(BinaryDataConstants.SYSTEM_IN_MEMORY_LIMIT);

            var base64 = global.GetBase64BinaryDataFromBinaryData(new BinaryDataContext(bytes));
            var decoded = global.GetBinaryDataFromBase64BinaryData(base64);

            base64.InMemory.Should().BeTrue();
            decoded.InMemory.Should().BeTrue();
            ReadAll(decoded).Should().Equal(bytes);
        }

        private static GlobalBinaryData Global(int limit)
        {
            var memoryLimit = Mock.Of<IBinaryDataMemoryLimit>(x => x.MaxBytesInMemory == limit);
            return (GlobalBinaryData)GlobalBinaryData.CreateInstance(memoryLimit);
        }

        // Лимит 1: все непустые данные лежат во временном файле
        private static BinaryDataContext InFile(byte[] bytes) => new BinaryDataContext(new MemoryStream(bytes), 1);

        private static byte[] RandomBytes(int count)
        {
            var bytes = new byte[count];
            new Random(42).NextBytes(bytes);
            return bytes;
        }

        private static byte[] ReadAll(BinaryDataContext data)
        {
            using var stream = data.GetStream();
            using var copy = new MemoryStream();
            stream.CopyTo(copy);
            return copy.ToArray();
        }
    }
}
