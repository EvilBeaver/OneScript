/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Работа с консолью")]
    public class ConsoleProvider : GlobalContextBase<ConsoleProvider>
    {
        private readonly ConsoleContext _console = new ConsoleContext();

        private ConsoleProvider()
        {
        }

        public override void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods)
        {
            variables = new [] {Variable.CreateContextPropertyReference(this, 0, GetPropName(0))};
            methods = new MethodInfo[0];
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
            if (!ReferenceEquals(newVal.GetRawValue(), _console))
            {
                throw new InvalidOperationException("Can't assign to global property Console");
            }
        }

        public static ConsoleProvider CreateInstance()
        {
            return new ConsoleProvider();
        }
    }
}