/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Compilation;
using OneScript.Compilation.Binding;
using ScriptEngine.Compiler;

namespace ScriptEngine
{
    /// <summary>
    /// Фасад для разделения старого компилятора и нового компилятора.
    /// Потенциально может быть удален после рефакторинга.
    /// Предоставляет CompilerService для Engine и оттуда - прикладному коду
    /// </summary>
    public interface ICompilerServiceFactory
    {
        ICompilerFrontend CreateInstance(SymbolTable context);
    }
}