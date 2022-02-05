/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    [ContextClass("ГенераторСлучайныхЧисел", "RandomNumberGenerator")]
    public class RandomNumberGenerator : AutoContext<RandomNumberGenerator>
    {
        private readonly Random _random;

        public RandomNumberGenerator()
        {
            _random = new Random();
        }

        public RandomNumberGenerator(int seed)
        {
            _random = new Random(seed);
        }

        [ContextMethod("СлучайноеЧисло", "RandomNumber")]
        public IValue RandomNumber(uint? low = null, uint? high = null)
        {
            uint lo = low !=null ? (uint)low : 0;
            uint hi = high != null ? (uint)high : uint.MaxValue;

            if (hi < lo)
                throw RuntimeException.InvalidArgumentValue();

            uint range = hi - lo;
            if (range == uint.MaxValue)
                return ValueFactory.Create( Random32() );

            // Приводим к рабочему диапазону
            long maxValue = int.MinValue + range + 1;

            long v = _random.Next(int.MinValue, (int)maxValue );
            v -= int.MinValue - lo;

            return ValueFactory.Create( v );
        }

        private uint Random32()
        {
            byte[] bytes = new byte[4];
            _random.NextBytes(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Формирует ГСЧ с возможностью указания начального числа.
        /// </summary>
        /// <param name="seed">Начальное число. Последовательность случайных чисел для одного и того же начального числа будет одинакова</param>
        /// <returns></returns>
        [ScriptConstructor(Name = "Конструктор по умолчанию")]
        public static RandomNumberGenerator Constructor(IValue seed)
        {
            decimal seedNum;
            try
            {
                seedNum = seed.GetRawValue().AsNumber();
            }
            catch
            {
                throw RuntimeException.InvalidArgumentType();
            }

            if (seedNum == 0)
                return new RandomNumberGenerator();

            int seedInt;
            if (seedNum < int.MinValue || seedNum > int.MaxValue)
            {
                var bits = decimal.GetBits(seedNum);
                seedInt = bits[0];
            }
            else
            {
                seedInt = (int)seedNum;
            }

            return new RandomNumberGenerator(seedInt);
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static RandomNumberGenerator Constructor()
        {
            return new RandomNumberGenerator();
        }

    }
}
