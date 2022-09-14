/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Sources;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class ThisAwareScriptedObjectBase : ScriptDrivenObject
    {
        protected const int THISOBJ_VARIABLE_INDEX = 0;
        private const int INVALID_INDEX = -1;
        
        protected const string THISOBJ_EN = "ThisObject";
        protected const string THISOBJ_RU = "ЭтотОбъект";

        protected ThisAwareScriptedObjectBase(IExecutableModule module, bool deferred) : base(module, deferred)
        {
        }
        
        protected  ThisAwareScriptedObjectBase(StackRuntimeModule module) : base(module)
        {
        }

        protected ThisAwareScriptedObjectBase()
        {
        }
        
        protected override int GetOwnVariableCount()
        {
            return 1;
        }
        
        protected override int FindOwnProperty(string name)
        {
            if (string.Compare(name, THISOBJ_RU, StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(name, THISOBJ_EN, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return THISOBJ_VARIABLE_INDEX;
            }

            return INVALID_INDEX;
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return true;

            throw new InvalidOperationException($"Property {index} should not be accessed through base class");
        }

        protected override bool IsOwnPropWritable(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return false;

            throw new InvalidOperationException($"Property {index} should not be accessed through base class");
        }

        protected override IValue GetOwnPropValue(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return this;

            throw new InvalidOperationException($"Property {index} should not be accessed through base class");
        }

        protected override string GetOwnPropName(int index)
        {
            return Locale.NStr($"ru='{THISOBJ_RU}';en='{THISOBJ_EN}'");
        }

        protected override BslPropertyInfo GetOwnPropertyInfo(int index)
        {
            if (index != THISOBJ_VARIABLE_INDEX)
                throw new InvalidOperationException($"Property {index} should not be accessed through base class");

            return BslPropertyBuilder.Create()
                .SetNames(THISOBJ_RU, THISOBJ_EN)
                .DeclaringType(GetType())
                .SetDispatchingIndex(index)
                .Build();
        }

        protected static void RegisterSymbols(ICompilerService compiler)
        {
            compiler.DefineVariable(THISOBJ_RU, THISOBJ_EN, SymbolType.ContextProperty);
        }
    }
}