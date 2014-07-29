using System;
using System.Collections.Generic;
using System.IO;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    public class ModuleWriter
    {
        CompilerService _compiler;

        public ModuleWriter(CompilerService compilerService)
        {
            _compiler = compilerService;
        }

        public void Write(TextWriter output, ICodeSource source)
        {
            var module = _compiler.CreateModule(source).Module;

            WriteImage(output, module);

        }

        public void Write(TextWriter output, ModuleHandle module)
        {
            WriteImage(output, module.Module);
        }

        private void WriteImage(TextWriter output, ModuleImage module)
        {
            output.WriteLine(".variableFrame:" + module.VariableFrameSize.ToString());
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

        private void WriteMethodDefinition(TextWriter output, MethodDescriptor item)
        {
            output.Write(item.Signature.IsFunction ? "Func " : "Proc ");
            output.Write(string.Format("{0} entryPoint: {1}, frameSize:{2}\n",
                item.Signature.Name,
                item.EntryPoint,
                item.VariableFrameSize));
            output.Write(string.Format(".args {0}\n", item.Signature.ArgCount));
            if (item.Signature.Params != null)
            {
                for (int i = 0; i < item.Signature.Params.Length; i++)
                {
                    output.Write(string.Format("{0,-3}: ByVal={1}", i, item.Signature.Params[i].IsByValue.ToString()));
                    if (item.Signature.Params[i].HasDefaultValue)
                    {
                        output.Write(string.Format(" defValue: {0}\n", item.Signature.Params[i].DefaultValueIndex));
                    }
                    else
                    {
                        output.WriteLine();
                    }
                }
            }
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
