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
