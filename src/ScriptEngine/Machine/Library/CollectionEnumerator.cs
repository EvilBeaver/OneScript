using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    class CollectionEnumerator : IValue, IEnumerator<IValue>
    {
        private IEnumerator<IValue> _iterator;

        public CollectionEnumerator(IEnumerator<IValue> realEnumerator)
        {
            _iterator = realEnumerator;
        }

        #region IEnumerator<IValue> Members

        public IValue Current
        {
            get { return _iterator.Current; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _iterator.Dispose();
            _iterator = null;
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current
        {
            get { return _iterator.Current; }
        }

        public bool MoveNext()
        {
            return _iterator.MoveNext();
        }

        public void Reset()
        {
            _iterator.Reset();
        }

        #endregion

        #region IValue Members

        public DataType DataType
        {
            get { throw new NotImplementedException(); }
        }

        public TypeDescriptor SystemType
        {
            get { throw new NotImplementedException(); }
        }

        public double AsNumber()
        {
            throw new NotImplementedException();
        }

        public DateTime AsDate()
        {
            throw new NotImplementedException();
        }

        public bool AsBoolean()
        {
            throw new NotImplementedException();
        }

        public string AsString()
        {
            throw new NotImplementedException();
        }

        public TypeDescriptor AsType()
        {
            throw new NotImplementedException();
        }

        public IRuntimeContextInstance AsObject()
        {
            throw new NotImplementedException();
        }

        public IValue GetRawValue()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IValue> Members

        public int CompareTo(IValue other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable<IValue> Members

        public bool Equals(IValue other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
