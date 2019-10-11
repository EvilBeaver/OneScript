/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptEngine;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class AutoScriptDrivenObject<T> : ScriptDrivenObject where T : AutoScriptDrivenObject<T>
    {
        private const int THISOBJ_VARIABLE_INDEX = 0;
        private const string THISOBJ_EN = "ThisObject";
        private const string THISOBJ_RU = "ЭтотОбъект";
        private const int PRIVATE_PROPS_OFFSET = 1;

        protected static readonly ContextPropertyMapper<T> _ownProperties = new ContextPropertyMapper<T>();
        protected static readonly ContextMethodsMapper<T> _ownMethods = new ContextMethodsMapper<T>();

        #region SDO Methods

        protected AutoScriptDrivenObject(LoadedModule module, bool deffered)
            : base(module, deffered)
        {
        }

        protected AutoScriptDrivenObject(LoadedModule module)
            : base(module)
        {
        }

        protected AutoScriptDrivenObject()
        {
        }

        protected override string GetOwnPropName(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return THISOBJ_RU;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).Name;
        }

        protected override int GetOwnVariableCount()
        {
            return _ownProperties.Count + PRIVATE_PROPS_OFFSET;
        }

        protected override int GetOwnMethodCount()
        {
            return _ownMethods.Count;
        }

        protected override void UpdateState()
        {
        }

        protected override int FindOwnProperty(string name)
        {
            if (string.Compare(name, THISOBJ_RU, StringComparison.OrdinalIgnoreCase) == 0
                || string.Compare(name, THISOBJ_EN, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return THISOBJ_VARIABLE_INDEX;
            }

            return _ownProperties.FindProperty(name) + PRIVATE_PROPS_OFFSET;
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return true;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).CanRead;
        }

        protected override bool IsOwnPropWritable(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return false;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).CanWrite;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            if (index == THISOBJ_VARIABLE_INDEX)
                return this;

            return _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).Getter((T)this);
        }

        protected override void SetOwnPropValue(int index, IValue val)
        {
            _ownProperties.GetProperty(index - PRIVATE_PROPS_OFFSET).Setter((T)this, val);
        }

        protected override int FindOwnMethod(string name)
        {
            try
            {
                int idx = _ownMethods.FindMethod(name);
                return idx;
            }
            catch (RuntimeException)
            {
                return -1;
            }
        }

        protected override MethodInfo GetOwnMethod(int index)
        {
            return _ownMethods.GetMethodInfo(index);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return _ownMethods.GetMethod(index)((T)this, arguments);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            _ownMethods.GetMethod(index)((T)this, arguments);
        }

        #endregion

        public static ModuleImage CompileModule(CompilerService compiler, ICodeSource src)
        {
            compiler.DefineVariable(THISOBJ_RU, THISOBJ_EN, SymbolType.ContextProperty);
            for (int i = 0; i < _ownProperties.Count; i++)
            {
                var currentProp = _ownProperties.GetProperty(i);
                compiler.DefineVariable(currentProp.Name, currentProp.Alias, SymbolType.ContextProperty);
            }

            for (int i = 0; i < _ownMethods.Count; i++)
            {
                compiler.DefineMethod(_ownMethods.GetMethodInfo(i));
            }

            return compiler.Compile(src);
        }
    }

}
