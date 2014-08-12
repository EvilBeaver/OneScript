using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class CompilerContext : OneScript.Scripting.ICompilerContext
    {
        List<SymbolScope> _attachedScopes = new List<SymbolScope>();

        
        public SymbolScope TopScope
        {
            get
            {
                return _attachedScopes[_attachedScopes.Count - 1];
            }
        }

        public void PushScope(SymbolScope scope)
        {
            _attachedScopes.Add(scope);
        }

        public SymbolScope PopScope()
        {
            var scope = TopScope;
            _attachedScopes.RemoveAt(_attachedScopes.Count - 1);
            return scope;
        }

        public SymbolBinding DefineVariable(string name)
        {
            try
            {
                int index = TopScope.DefineVariable(name);
                var binding = new SymbolBinding();
                binding.Name = name;
                binding.Context = _attachedScopes.Count - 1;
                binding.IndexInContext = index;

                return binding;
            }
            catch (ArgumentException e)
            {
                throw new CompilerException(e.Message);
            }
        }

        public bool IsVarDefined(string name)
        {
            for (int i = _attachedScopes.Count-1; i >= 0; i--)
            {
                if (_attachedScopes[i].IsVarDefined(name))
                {
                    return true;
                }
            }

            return false;
        }

        public SymbolBinding GetVariable(string name)
        {
            for (int i = _attachedScopes.Count - 1; i >= 0; i--)
            {
                int varIndex = _attachedScopes[i].GetVariableNumber(name);
                if (varIndex != SymbolScope.InvalidIndex)
                {
                    var binding = new SymbolBinding();
                    binding.Context = i;
                    binding.IndexInContext = varIndex;
                    binding.Name = name;

                    return binding;
                }
            }

            throw CompilerException.VariableIsNotDefined(name);
        }

        public SymbolBinding DefineMethod(string name)
        {
            try
            {
                int index = TopScope.DefineMethod(name, MethodUsageData.CreateProcedure(0));
                var binding = new SymbolBinding();
                binding.Name = name;
                binding.Context = _attachedScopes.Count - 1;
                binding.IndexInContext = index;

                return binding;
            }
            catch (ArgumentException e)
            {
                throw new CompilerException(e.Message);
            }
        }

        public bool IsMethodDefined(string name)
        {
            for (int i = _attachedScopes.Count - 1; i >= 0; i--)
            {
                if (_attachedScopes[i].IsMethodDefined(name))
                {
                    return true;
                }
            }

            return false;
        }

        public SymbolBinding GetMethod(string name)
        {
            for (int i = _attachedScopes.Count - 1; i >= 0; i--)
            {
                int varIndex = _attachedScopes[i].GetMethodNumber(name);
                if (varIndex != SymbolScope.InvalidIndex)
                {
                    var binding = new SymbolBinding();
                    binding.Context = i;
                    binding.IndexInContext = varIndex;
                    binding.Name = name;

                    return binding;
                }
            }

            throw CompilerException.MethodIsNotDefined(name);
        }


        public object GetScope(int number)
        {
            return _attachedScopes[number];
        }
    }
}
