/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using BenchmarkDotNet.Attributes;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.Benchmarks
{
    /// <summary>
    /// Вызов метода контекста из C#, без стековой машины: обертка метода и преобразование значений
    /// </summary>
    [MemoryDiagnoser]
    public class ContextCallBenchmarks
    {
        private readonly IBslProcess _process = ForbiddenBslProcess.Instance;
        private readonly IValue _number = ValueFactory.Create(5);
        private readonly IValue _string = ValueFactory.Create("строка");
        private bool _flag = true;
        private int _integer = 5;

        private ArrayImpl _array;
        private MapImpl _map;
        private int _setMethod;
        private int _countMethod;
        private int _getMethod;
        private int _mapGetMethod;
        private IValue[] _setArguments;
        private IValue[] _getArguments;
        private IValue[] _mapGetArguments;

        [GlobalSetup]
        public void Setup()
        {
            _array = new ArrayImpl();
            _array.Add(_number);
            _setMethod = _array.GetMethodNumber("Установить");
            _countMethod = _array.GetMethodNumber("Количество");
            _getMethod = _array.GetMethodNumber("Получить");
            _setArguments = new[] { ValueFactory.Create(0), _number };
            _getArguments = new[] { ValueFactory.Create(0) };

            _map = new MapImpl();
            _map.Insert(_number, _number);
            _mapGetMethod = _map.GetMethodNumber("Получить");
            _mapGetArguments = new[] { _number };
        }

        [Benchmark]
        public int ConvertParamInt() => ContextValuesMarshaller.ConvertParam(_number, 0, _process);

        [Benchmark]
        public string ConvertParamString() => ContextValuesMarshaller.ConvertParam(_string, "", _process);

        [Benchmark]
        public IValue ConvertParamValue() => ContextValuesMarshaller.ConvertParam<IValue>(_number, null, _process);

        [Benchmark]
        public IValue ConvertReturnBool() => ContextValuesMarshaller.ConvertReturnValue(_flag);

        [Benchmark]
        public IValue ConvertReturnInt() => ContextValuesMarshaller.ConvertReturnValue(_integer);

        [Benchmark]
        public IValue ConvertReturnValue() => ContextValuesMarshaller.ConvertReturnValue(_number);

        [Benchmark]
        public void ArraySet() => _array.CallAsProcedure(_setMethod, _setArguments, _process);

        [Benchmark]
        public IValue ArrayCount()
        {
            _array.CallAsFunction(_countMethod, Array.Empty<IValue>(), out var result, _process);
            return result;
        }

        [Benchmark]
        public IValue ArrayGet()
        {
            _array.CallAsFunction(_getMethod, _getArguments, out var result, _process);
            return result;
        }

        [Benchmark]
        public IValue MapGet()
        {
            _map.CallAsFunction(_mapGetMethod, _mapGetArguments, out var result, _process);
            return result;
        }
    }
}
