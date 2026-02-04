/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;
using OneScript.Execution;
using OneScript.Types;

namespace ScriptEngine.Machine
{
    public class GenericIValueComparer : IEqualityComparer<IValue>, IComparer<IValue>
    {
        private readonly IBslProcess _process;
        private readonly Func<IValue, IValue, int> _comparer;

        static private readonly List<TypeDescriptor>orderedTypes = new List<TypeDescriptor>
                { BasicTypes.Undefined, BasicTypes.Null, BasicTypes.Boolean,
                BasicTypes.Number, BasicTypes.String, BasicTypes.Date, BasicTypes.Type };

        private const int INDEX_OF_TYPE = 6;

        public GenericIValueComparer()
        {
            _comparer = CompareAsStrings;
        }

        public GenericIValueComparer(IBslProcess proc)
        {
            _process = proc;
            _comparer = CompareByPresentations;
        }

        public bool Equals(IValue x, IValue y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IValue obj)
        {
            object CLR_obj;
            if (obj is BslUndefinedValue)
                return obj.GetHashCode();

            if (obj is BslNullValue)
                return obj.GetHashCode();

            try
            {
                CLR_obj = ContextValuesMarshaller.ConvertToClrObject(obj);
            }
            catch (ValueMarshallingException)
            {
                CLR_obj = obj;
            }

            if (CLR_obj == null)
                return 0;

            return CLR_obj.GetHashCode();
        }

        private int CompareAsStrings(IValue x, IValue y)
        {
            return x.ToString().CompareTo(y.ToString());
        }

        private int CompareByPresentations(IValue x, IValue y)
        {
            return ((BslValue)x).ToString(_process).CompareTo(((BslValue)y).ToString(_process));
        }

        /// <summary>
        /// Сравнение переменных разных типов. Правила сравнения соответствуют 1С v8.3.27:
        /// Переменные типа "Тип" следуют за всеми прочими;
        /// Другие примитивные типы предшествуют объектным типам;
        /// Между собой примитивные типы упорядочиваются в последовательности, указанной в orderedTypes;
        /// Объектные типы упорядочиваются по строковому представлению.
        /// </summary>
        private int CompareByTypes(IValue x, IValue y)
        {
            var ix = orderedTypes.IndexOf(x.SystemType);
            var iy = orderedTypes.IndexOf(y.SystemType);

            if (ix >= 0)
                if (iy >= 0)
                    return ix - iy;
                else
                    return ix == INDEX_OF_TYPE ? 1 : -1;
            else if (iy >= 0)
                return iy == INDEX_OF_TYPE ? -1 : 1;

            return _comparer(x,y);
        }

        public int Compare(IValue x, IValue y)
        {
           if (ReferenceEquals(x, y))
                return 0;

            if (x.SystemType != y.SystemType )
                return CompareByTypes(x,y);

            if (x is IBslComparable)
                return x.CompareTo(y);
            else
                return _comparer(x,y);
        }
    }
}
