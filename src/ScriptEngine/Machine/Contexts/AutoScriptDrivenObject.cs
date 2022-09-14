/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Sources;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class AutoScriptDrivenObject<T> : ThisAwareScriptedObjectBase where T : AutoScriptDrivenObject<T>
    {
        private readonly int _privatePropsOffset;

        protected static readonly ContextPropertyMapper<T> _ownProperties = new ContextPropertyMapper<T>();
        protected static readonly ContextMethodsMapper<T> _ownMethods = new ContextMethodsMapper<T>();

        #region SDO Methods

        protected AutoScriptDrivenObject(IExecutableModule module, bool deferred)
            : base(module, deferred)
        {
            _privatePropsOffset = base.GetOwnVariableCount();
            if (!deferred)
                InitOwnData();
        }

        protected AutoScriptDrivenObject(IExecutableModule module)
            : this(module, false)
        {
        }

        protected AutoScriptDrivenObject() : this(EmptyModule.Instance)
        {
        }

        protected override string GetOwnPropName(int index)
        {
            if (index < _privatePropsOffset)
                return base.GetOwnPropName(index);

            return _ownProperties.GetProperty(index - _privatePropsOffset).Name;
        }

        protected override int GetOwnVariableCount()
        {
            return _ownProperties.Count + _privatePropsOffset;
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
            var baseIndex = base.FindOwnProperty(name);
            if (baseIndex != -1)
            {
                return baseIndex;
            }

            return _ownProperties.FindProperty(name) + _privatePropsOffset;
        }

        protected override bool IsOwnPropReadable(int index)
        {
            if (index < _privatePropsOffset)
                return base.IsOwnPropReadable(index);

            return _ownProperties.GetProperty(index - _privatePropsOffset).CanRead;
        }

        protected override bool IsOwnPropWritable(int index)
        {
            if (index < _privatePropsOffset)
                return false;

            return _ownProperties.GetProperty(index - _privatePropsOffset).CanWrite;
        }

        protected override IValue GetOwnPropValue(int index)
        {
            if (index < _privatePropsOffset)
                return this;

            return _ownProperties.GetProperty(index - _privatePropsOffset).Getter((T)this);
        }

        protected override void SetOwnPropValue(int index, IValue val)
        {
            _ownProperties.GetProperty(index - _privatePropsOffset).Setter((T)this, val);
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

        protected override BslMethodInfo GetOwnMethod(int index)
        {
            return _ownMethods.GetRuntimeMethod(index);
        }

        protected override IValue CallOwnFunction(int index, IValue[] arguments)
        {
            return _ownMethods.GetCallableDelegate(index)((T)this, arguments);
        }

        protected override void CallOwnProcedure(int index, IValue[] arguments)
        {
            _ownMethods.GetCallableDelegate(index)((T)this, arguments);
        }

        #endregion

        protected new static void RegisterSymbols(ICompilerService compiler)
        {
            for (int i = 0; i < _ownProperties.Count; i++)
            {
                var currentProp = _ownProperties.GetProperty(i);
                compiler.DefineVariable(currentProp.Name, currentProp.Alias, SymbolType.ContextProperty);
            }

            for (int i = 0; i < _ownMethods.Count; i++)
            {
                compiler.DefineMethod(_ownMethods.GetRuntimeMethod(i));
            }
        }
        
        public static IExecutableModule CompileModule(ICompilerService compiler, SourceCode src)
        {
            ThisAwareScriptedObjectBase.RegisterSymbols(compiler);
            RegisterSymbols(compiler);

            return compiler.Compile(src);
        }
    }

}
