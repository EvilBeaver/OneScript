/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.Contexts;

namespace ScriptEngine.Machine
{
    internal class MachineMethodInfo : BslScriptMethodInfo
    {
        private MachineMethod _method;
        
        // Внутренний конструктор для десериализации из кэша
        internal MachineMethodInfo(MachineMethod method) : base()
        {
            _method = method;
        }
        
        public MachineMethodInfo() : base()
        {
            // Конструктор по умолчанию для обычного создания
        }
        
        internal void SetRuntimeParameters(int entryPoint, string[] locals)
        {
            _method = new MachineMethod
            {
                EntryPoint = entryPoint,
                LocalVariables = locals,
                Signature = this.MakeSignature()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MachineMethod GetRuntimeMethod() => _method;
    }
}