/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;

namespace ScriptEngine.Machine
{
    /// <summary>
    /// Потоково-асинхронный счетчик подавления предупреждений об устаревании
    /// при диагностических чтениях (например, визуализатором переменных отладчика).
    /// Использование:
    /// using (DeprecationWarningScope.Suppress()) { /* диагностическое чтение */ }
    /// </summary>
    public static class DeprecationWarningScope
    {
        private static readonly AsyncLocal<int> _suppressCounter = new AsyncLocal<int>();

        public static bool IsSuppressed => _suppressCounter.Value > 0;

        public static IDisposable Suppress()
        {
            return new ScopeToken();
        }

        private sealed class ScopeToken : IDisposable
        {
            private bool _disposed;

            public ScopeToken()
            {
                _suppressCounter.Value = _suppressCounter.Value + 1;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                var current = _suppressCounter.Value;
                _suppressCounter.Value = current > 0 ? current - 1 : 0;
            }
        }
    }
}