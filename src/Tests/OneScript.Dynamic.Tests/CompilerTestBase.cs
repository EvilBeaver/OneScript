/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Compilation.Binding;
using OneScript.Language;
using OneScript.Native.Compiler;

namespace OneScript.Dynamic.Tests;

public class CompilerTestBase
{
    protected DynamicModule CreateModule(string code)
    {
        var helper = new CompileHelper();
        helper.ParseModule(code);
        var result = helper.Compile(new SymbolTable());
        helper.ThrowOnErrors();
        return result;
    }

    protected DynamicModule CreateModule(string code, List<CodeError> errors)
    {
        var helper = new CompileHelper();
        helper.ParseModule(code);
        var result = helper.Compile(new SymbolTable());
        errors.AddRange(helper.Errors);
        return result;
    }
}