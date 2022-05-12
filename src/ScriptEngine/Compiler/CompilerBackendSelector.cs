/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using System.Linq.Expressions;
using OneScript.Compilation;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using ScriptEngine.Compiler;

namespace ScriptEngine.HostedScript
{
    public class CompilerBackendSelector : BslSyntaxWalker
    {
        private bool _isNative;
        
        public Func<StackMachineCodeGenerator> StackBackendInitializer { get; set; }
        
        public Func<ModuleCompiler> NativeBackendInitializer { get; set; }

        public ICompilerBackend Select(ModuleNode ast)
        {
            VisitModule(ast);
            if (_isNative)
            {
                return NativeBackendInitializer?.Invoke();
            }
            
            return StackBackendInitializer?.Invoke();
        }
        
        protected override void VisitModuleAnnotation(AnnotationNode node)
        {
            if (string.Equals(node.Name, NativeRuntimeAnnotationHandler.NativeDirectiveName,
                StringComparison.CurrentCultureIgnoreCase))
            {
                _isNative = true;
            }
        }

        protected override void VisitModule(ModuleNode node)
        {
            foreach (var child in node.Children.Where(x => x.Kind == NodeKind.Annotation))
            {
                VisitModuleAnnotation((AnnotationNode)child);
            }
        }
    }
}