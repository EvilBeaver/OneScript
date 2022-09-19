/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using ScriptEngine.Machine;
using OneScript.Sources;

namespace ScriptEngine
{
    /// <summary>
    /// Сервис компиляции единичного модуля.
    /// </summary>
    public interface ICompilerService
    {
        bool GenerateDebugCode { get; set; }
        
        bool GenerateCodeStat { get; set; }
        
        int DefineVariable(string name, string alias, SymbolType type);
        
        int DefineMethod(BslMethodInfo methodSignature);
        
        void DefinePreprocessorValue(string name);
        
        IExecutableModule Compile(SourceCode source, Type classType = null);
        
        IExecutableModule CompileExpression(SourceCode source);
        
        IExecutableModule CompileBatch(SourceCode source);
    }

    /// <summary>
    /// Заглушка на стадии рефакторинга инфраструктуры компилятора
    /// </summary>
    public static class StackCompilerExtension
    {
        public static StackRuntimeModule CompileStack(this ICompilerService compiler, SourceCode source, Type classType = null)
        {
            return (StackRuntimeModule)compiler.Compile(source, classType);
        }
    }
}