/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Sources;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public class ModuleDumpWriter
    {
        readonly ICompilerService _compiler;

        public ModuleDumpWriter(ICompilerService compilerService)
        {
            _compiler = compilerService;
        }

        public void Write(TextWriter output, SourceCode source)
        {
            var module = _compiler.Compile(source);

            WriteImage(output, module);

        }

        public void Write(TextWriter output, StackRuntimeModule module)
        {
            WriteImage(output, module);
        }

        private void WriteImage(TextWriter output, StackRuntimeModule module)
        {
            output.WriteLine(".loadAt: {0}", module.LoadAddress);
            output.WriteLine(".variableFrame:");
            module.Fields
                .Cast<BslScriptFieldInfo>()
                .OrderBy(x=>x.DispatchId)
                .ForEach(x=>output.WriteLine($"{x.DispatchId}:{x.Name}{(x.IsPublic ? " export" : "")}"));
            
            output.WriteLine(".constants");
            for (int i = 0; i < module.Constants.Count; i++)
            {
                var item = module.Constants[i];
                output.WriteLine(
                    $"{i,-3}:type: {item.SystemType.Alias}, val: {item}");
            }
            output.WriteLine(".code");
            for (int i = 0; i < module.Code.Count; i++)
            {
                var item = module.Code[i];
                output.WriteLine(
                    $"{i,-3}:({Enum.GetName(typeof(OperationCode), item.Code),-10}{item.Argument,3})");
            }
            output.WriteLine(".procedures");
            foreach (var item in module.Methods.Cast<MachineMethodInfo>())
            {
                WriteMethodDefinition(output, item.GetRuntimeMethod());
            }
            output.WriteLine(".varmap");
            WriteSymbolMap(output, module.VariableRefs);
            output.WriteLine(".procmap");
            WriteSymbolMap(output, module.MethodRefs);
            output.WriteLine(".moduleEntry");
            output.WriteLine(module.EntryMethodIndex.ToString());
        }

        private void WriteSymbolMap(TextWriter output, IList<SymbolBinding> map)
        {
            for (int i = 0; i < map.Count; i++)
            {
                var item = map[i];
                output.Write(string.Format("{0,-3}:({1},{2})\n", i, item.ContextIndex, item.CodeIndex));
            }
        }

        private void WriteAnnotationsList(TextWriter output, AnnotationDefinition[] annotations)
        {
            output.WriteLine(".annotations [");
            foreach (var annotation in annotations)
            {
                output.Write(" {0}", annotation.Name);
                if (annotation.ParamCount != 0)
                {
                    var delimiter = ": ";
                    foreach (var parameter in annotation.Parameters)
                    {
                        output.Write(delimiter);
                        output.Write(parameter);
                        delimiter = ", ";
                    }
                }
                output.WriteLine("");
            }
            output.WriteLine("]");
        }

        private void WriteMethodDefinition(TextWriter output, MachineMethod item)
        {
            output.Write(item.Signature.IsFunction ? "Func " : "Proc ");
            output.Write(
                $"{item.Signature.Name} entryPoint: {item.EntryPoint}, frameSize:{item.LocalVariables.Length}\n");
            if (item.Signature.AnnotationsCount != 0)
            {
                WriteAnnotationsList(output, item.Signature.Annotations);
            }
            output.Write(".args {0}\n", item.Signature.ArgCount);
            if (item.Signature.Params != null)
            {
                for (int i = 0; i < item.Signature.Params.Length; i++)
                {
                    output.Write("{0,-3}: ByVal={1}", i, item.Signature.Params[i].IsByValue);
                    if (item.Signature.Params[i].HasDefaultValue)
                    {
                        output.Write(" defValue: {0}\n", item.Signature.Params[i].DefaultValueIndex);
                    }
                    else
                    {
                        output.WriteLine();
                    }
                }
            }
            output.WriteLine(".variables [");
            for (int i = 0; i < item.LocalVariables.Length; i++)
            {
                output.WriteLine($" {i}:{item.LocalVariables[i]}");
            }
            output.WriteLine("]");
        }
    }
}
