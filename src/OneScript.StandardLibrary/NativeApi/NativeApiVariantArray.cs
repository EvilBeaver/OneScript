/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Exceptions;

namespace OneScript.StandardLibrary.NativeApi
{
    /// <summary>
    /// Владелец непрерывного массива tVariant из состава Native API
    /// </summary>
    sealed class NativeApiVariantArray : IDisposable
    {
        private IntPtr _ptr = IntPtr.Zero;

        public NativeApiVariantArray(int count)
        {
            Count = count;
            if (count <= 0)
                return;

            _ptr = NativeApiProxy.CreateVariant(count);
            if (_ptr == IntPtr.Zero)
                throw new RuntimeException("Не удалось выделить память для параметров Native API");
        }

        public IntPtr Ptr => _ptr;

        public int Count { get; }

        public NativeApiVariant this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                var elementPtr = NativeApiProxy.VariantElement(_ptr, index);
                if (elementPtr == IntPtr.Zero)
                    throw new RuntimeException("Не удалось получить элемент массива Native API");

                return new NativeApiVariant(elementPtr);
            }
        }

        public void Dispose()
        {
            if (_ptr != IntPtr.Zero)
            {
                NativeApiProxy.FreeVariant(_ptr, Count);
                _ptr = IntPtr.Zero;
            }
        }
    }
}
