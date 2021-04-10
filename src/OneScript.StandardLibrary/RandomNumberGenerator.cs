/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
{
    [ContextClass("ГенераторСлучайныхЧисел", "RandomNumberGenerator")]
    public class RandomNumberGenerator : AutoContext<RandomNumberGenerator>
    {
        private readonly Random _random;

        public RandomNumberGenerator(int seed = 0)
        {
            if (seed == 0)
                _random = new Random();
            else
                _random = new Random(seed);
        }

        [ContextMethod("СлучайноеЧисло", "RandomNumber")]
        public IValue RandomNumber(IValue low = null, IValue high = null)
        {
            long lo64 = 0, hi64 = UInt32.MaxValue;

            if (low != null)
                lo64 = decimal.ToInt64(low.AsNumber());

            if (high != null)
                hi64 = decimal.ToInt64(high.AsNumber());

            if (lo64 < 0 || lo64 > 4294967295)
                throw RuntimeException.InvalidArgumentValue();

            if (hi64 < 0 || hi64 > 4294967295)
                throw RuntimeException.InvalidArgumentValue();

            if (hi64 < lo64)
                throw RuntimeException.InvalidArgumentValue();

            // Приводим к рабочему диапазону
            lo64 += Int32.MinValue;
            hi64 += Int32.MinValue;

            int lo = (int)lo64, hi = (int)hi64;

            int v = _random.Next(lo, hi);
            long v64 = v;
            v64 -= Int32.MinValue;

            return ValueFactory.Create( v64 );
        }

        /// <summary>
        /// Формирует ГСЧ с возможностью указания начального числа.
        /// </summary>
        /// <param name="seed">Начальное число. Последовательность случайных чисел для одного и того же начального числа будет одинакова</param>
        /// <returns></returns>
        [ScriptConstructor(Name = "Конструктор по умолчанию")]
        public static RandomNumberGenerator Constructor(IValue seed)
        {
            seed = seed.GetRawValue();
            if (seed.DataType == DataType.Number)
                return new RandomNumberGenerator(decimal.ToInt32(seed.AsNumber()));

            return new RandomNumberGenerator();
        }

        [ScriptConstructor(Name = "Формирование неинициализированного объекта")]
        public static RandomNumberGenerator Constructor()
        {
            return new RandomNumberGenerator();
        }

    }
}
