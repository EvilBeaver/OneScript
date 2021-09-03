/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;
using OneScript.Contexts;

namespace ScriptEngine.Machine.Contexts
{
    /// <summary>
    /// Класс предназначен для блокировки потока для монопольного доступа к ресурсу 
    /// </summary>
    [ContextClass("БлокировкаРесурса", "ResourceLock")]
    public class CriticalSectionContext : AutoContext<CriticalSectionContext>, IDisposable
    {
        private object _lockObject;
        
        private CriticalSectionContext()
        {
            _lockObject = new object();
        }

        private CriticalSectionContext(object lockObject)
        {
            _lockObject = lockObject;
        }

        [ContextMethod("Заблокировать", "Lock")]
        public void Lock()
        {
            Monitor.Enter(_lockObject);
        }
        
        [ContextMethod("Разблокировать", "Unlock")]
        public void Unlock()
        {
            Monitor.Exit(_lockObject);
        }
        
        public void Dispose()
        {
            if(Monitor.IsEntered(_lockObject))
                Monitor.Exit(_lockObject);
            
            _lockObject = null;
        }

        [ScriptConstructor]
        public static CriticalSectionContext Create()
        {
            return new CriticalSectionContext();
        }
        
        [ScriptConstructor(Name = "По объекту")]
        public static CriticalSectionContext Create(IValue instance)
        {
            return new CriticalSectionContext(instance.AsObject());
        }
    }
}