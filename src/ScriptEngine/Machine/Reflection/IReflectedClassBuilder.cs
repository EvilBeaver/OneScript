/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;

namespace ScriptEngine.Machine.Reflection
{
    public interface IReflectedClassBuilder
    {
        IReflectedClassBuilder SetTypeName(string typeName);

        IReflectedClassBuilder SetModule(LoadedModule module);

        IReflectedClassBuilder ExportClassMethod(string methodName);

        IReflectedClassBuilder ExportClassMethod(System.Reflection.MethodInfo nativeMethod);

        IReflectedClassBuilder ExportProperty(string propName);

        IReflectedClassBuilder ExportMethods(bool includeDeprecations = false);

        IReflectedClassBuilder ExportProperties(bool includeDeprecations = false);

        IReflectedClassBuilder ExportConstructor(System.Reflection.ConstructorInfo info);

        IReflectedClassBuilder ExportConstructor(Func<object[], IRuntimeContextInstance> creator);

        IReflectedClassBuilder ExportScriptVariables();

        IReflectedClassBuilder ExportScriptMethods();

        IReflectedClassBuilder ExportScriptConstructors();

        IReflectedClassBuilder ExportDefaults();

        Type Build();
    }
}