/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    [GlobalContext(Category = "Работа с консолью")]
    public class ConsoleProvider : GlobalContextBase<ConsoleProvider>
    {
        private readonly ConsoleContext _console;

        private ConsoleProvider(ExecutionContext executionContext)
        {
            _console = new ConsoleContext(executionContext);
        }
        
        [ContextProperty("Консоль", "Console")]
        public ConsoleContext Console => _console;

        public override bool IsPropWritable(int propNum)
        {
            // обратная совместимость. Присваивание Консоль = Новый Консоль не должно ругаться на недоступность записи
            return true;
        }
        
        public override void SetPropValue(int propNum, IValue newVal)
        {
            // обратная совместимость. Присваивание Консоль = Новый Консоль не должно ничего делать
            if (!ReferenceEquals(newVal, _console))
            {
                throw new InvalidOperationException("Can't assign to global property Console");
            }
        }

        public static ConsoleProvider CreateInstance(ExecutionContext executionContext)
        {
            return new ConsoleProvider(executionContext);
        }
    }
}