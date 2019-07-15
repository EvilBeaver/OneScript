/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    public class CompilerService
    {
        SymbolScope _scope;

        readonly ModuleCompilerContext _currentContext;

        readonly List<int> _predefinedVariables = new List<int>();

        internal CompilerService(CompilerContext outerContext)
        {
            _currentContext = new ModuleCompilerContext(outerContext);
        }

        public CodeGenerationFlags ProduceExtraCode { get; set; }

        public int DefineVariable(string name, string alias, SymbolType type)
        {
            RegisterScopeIfNeeded();

            try
            {
                int varIdx;
                if (type == SymbolType.Variable)
                    varIdx = _currentContext.DefineVariable(name, alias).CodeIndex;
                else
                    varIdx = _currentContext.DefineProperty(name, alias).CodeIndex;

                _predefinedVariables.Add(varIdx);
                return varIdx;
            }
            catch
            {
                _currentContext.PopScope();
                _scope = null;
                throw;
            }
        }

        public int DefineMethod(MethodInfo methodInfo)
        {
            RegisterScopeIfNeeded();

            return _currentContext.DefineMethod(methodInfo).CodeIndex;
        }

        private void RegisterScopeIfNeeded()
        {
            if (_scope == null)
            {
                _scope = new SymbolScope();
                _currentContext.PushScope(_scope);
            }
        }

        public ModuleImage Compile(ICodeSource source)
        {
            try
            {
                return CompileInternal(source);
            }
            finally
            {
                _currentContext.PopScope();
                _scope = null;
            }
        }

        private ModuleImage CompileInternal(ICodeSource source)
        {
            RegisterScopeIfNeeded();

            var parser = new PreprocessingLexer();
            parser.Code = source.Code;

            var compiler = new Compiler.Compiler();
            compiler.ProduceExtraCode = ProduceExtraCode;
            compiler.DirectiveHandler = ResolveDirective;

            if (DirectiveResolver != null)
            {
                DirectiveResolver.Source = source;
            }

            ModuleImage compiledImage;
            try
            {
                compiledImage = compiler.Compile(parser, _currentContext);
            }
            catch (ScriptException e)
            {
                if(e.ModuleName == null)
                    e.ModuleName = source.SourceDescription;

                throw;
            }
            finally
            {
                if (DirectiveResolver != null)
                {
                    DirectiveResolver.Source = null;
                }
            }

            var mi = new ModuleInformation();
            mi.CodeIndexer = parser.Iterator;
            // пока у модулей нет собственных имен, будет совпадать с источником модуля
            mi.ModuleName = source.SourceDescription;
            mi.Origin = source.SourceDescription;
            compiledImage.ModuleInfo = mi;

            return compiledImage;
        }

        private bool ResolveDirective(string directive, string value, bool codeEntered)
        {
            if (DirectiveResolver != null)
            {
                return DirectiveResolver.Resolve(directive, value, codeEntered);
            }
            else
                return false;
        }

        public IDirectiveResolver DirectiveResolver { get; set; }
    }
}
