/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.DebugServices;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Machine;
using Variable = OneScript.DebugProtocol.Variable;
using MachineVariable = ScriptEngine.Machine.Variable;

namespace ScriptEngine.HostedScript.DebugCustomizations
{
    public class CollectionAwareVariableVisualizer : IVariableVisualizer
    {
        private readonly IVariableVisualizer _visualizer;

        public CollectionAwareVariableVisualizer() : this(new DefaultVariableVisualizer())
        {
        }
        
        public CollectionAwareVariableVisualizer(IVariableVisualizer visualizer)
        {
            _visualizer = visualizer;
        }
        
        public Variable GetVariable(IVariable value) => _visualizer.GetVariable(value);

        public IEnumerable<Variable> GetChildVariables(IValue value)
        {
            if (value.DataType == DataType.Object)
            {
                var context = value.AsObject();
                if (context is StructureImpl)
                {
                    return OnlyProperties(context);
                }

                return _visualizer.GetChildVariables(value);
            }

            return _visualizer.GetChildVariables(value);
        }

        private IEnumerable<Variable> OnlyProperties(IRuntimeContextInstance context)
        {
            var props = context.GetProperties();
            foreach (var prop in props)
            {
                var value = context.GetPropValue(prop.Index);
                yield return _visualizer.GetVariable(MachineVariable.Create(value, prop.Identifier));
            }
        }
    }
}