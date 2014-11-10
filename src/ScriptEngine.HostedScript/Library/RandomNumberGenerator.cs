using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("ГенераторСлучайныхЧисел", "RandomNumberGenerator")]
    class RandomNumberGenerator : AutoContext<RandomNumberGenerator>
    {
        private Random _random;

        public RandomNumberGenerator(int seed = 0)
        {
            if (seed == 0)
                _random = new Random();
            else
                _random = new Random(seed);
        }

        [ContextMethod("СлучайноеЧисло", "RandomNumber")]
        public int RandomNumber(IValue low = null, IValue high = null)
        {
            // TODO: должно работать с диапазоном 0..4294967295
            // TODO: Проверка типов

            int lo = 0, hi = int.MaxValue;

            if (low != null)
                lo = decimal.ToInt32(low.AsNumber());

            if (high != null)
                hi = decimal.ToInt32(high.AsNumber());

            return _random.Next(lo, hi);
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue seed)
        {
            seed = seed.GetRawValue();
            if (seed.DataType == DataType.Number)
                return new RandomNumberGenerator(decimal.ToInt32(seed.AsNumber()));

            return new RandomNumberGenerator();
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new RandomNumberGenerator();
        }

    }
}
