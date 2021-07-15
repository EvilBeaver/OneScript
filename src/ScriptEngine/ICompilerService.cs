/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using OneScript.Sources;

namespace ScriptEngine
{
    /// <summary>
    /// Сервис компиляции единичного модуля.
    /// </summary>
    public interface ICompilerService
    {
        CodeGenerationFlags ProduceExtraCode { get; set; }
        
        int DefineVariable(string name, string alias, SymbolType type);
        
        int DefineMethod(MethodSignature methodSignature);
        
        void DefinePreprocessorValue(string name);
        
        ModuleImage Compile(ICodeSource source);
        
        ModuleImage CompileExpression(ICodeSource source);
        
        ModuleImage CompileBatch(ICodeSource source);
    }
}