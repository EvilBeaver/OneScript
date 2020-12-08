﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public class ModuleWriter
    {
        readonly ICompilerService _compiler;

        public ModuleWriter(ICompilerService compilerService)
        {
            _compiler = compilerService;
        }

        public void Write(TextWriter output, ICodeSource source)
        {
            var module = _compiler.Compile(source);

            WriteImage(output, module);

        }

        public void Write(TextWriter output, ModuleImage module)
        {
            WriteImage(output, module);
        }

        private void WriteImage(TextWriter output, ModuleImage module)
        {
            output.WriteLine(".loadAt: {0}", module.LoadAddress);
            output.WriteLine(".variableFrame:");
            module.Variables.ForEach(x=>output.WriteLine(" " + x));

            output.WriteLine(".constants");
            for (int i = 0; i < module.Constants.Count; i++)
            {
                var item = module.Constants[i];
                output.WriteLine(
                    String.Format("{0,-3}:type: {1}, val: {2}",
                    i,
                    Enum.GetName(typeof(DataType), item.Type),
                    item.Presentation));
            }
            output.WriteLine(".code");
            for (int i = 0; i < module.Code.Count; i++)
            {
                var item = module.Code[i];
                output.WriteLine(
                    String.Format("{0,-3}:({1,-10}{2,3})",
                    i,
                    Enum.GetName(typeof(OperationCode), item.Code),
                    item.Argument));
            }
            output.WriteLine(".procedures");
            foreach (var item in module.Methods)
            {
                WriteMethodDefinition(output, item);
            }
            output.WriteLine(".varmap");
            WriteSymbolMap(output, module.VariableRefs);
            output.WriteLine(".procmap");
            WriteSymbolMap(output, module.MethodRefs);
            output.WriteLine(".moduleEntry");
            output.WriteLine(module.EntryMethodIndex.ToString());
            output.WriteLine(".exports");
            WriteExports(output, module.ExportedProperties);
            WriteExports(output, module.ExportedMethods);
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

        private void WriteMethodDefinition(TextWriter output, MethodDescriptor item)
        {
            output.Write(item.Signature.IsFunction ? "Func " : "Proc ");
            output.Write(string.Format("{0} entryPoint: {1}, frameSize:{2}\n",
                item.Signature.Name,
                item.EntryPoint,
                item.Variables.Count));
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
            item.Variables.ForEach(x=>output.WriteLine(" " + x));
            output.WriteLine("]");
        }

        private void WriteExports(TextWriter output, IList<ExportedSymbol> exports)
        {
            for (int i = 0; i < exports.Count; i++)
            {
                output.WriteLine(String.Format("{0}:{1,-3}", exports[i].SymbolicName, exports[i].Index));
            }
        }
    }
}
