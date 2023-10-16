﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using OneScript.Compilation;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;

namespace ScriptEngine.Compiler
{
    public class CompilerBackendSelector : BslSyntaxWalker
    {
        private bool _isNative;
        private bool _isNativeDefault;

        public CompilerBackendSelector(OneScriptCoreOptions options)
        {
            _isNativeDefault = options.UseNativeAsDefaultRuntime;
        }
        
        public Func<ICompilerBackend> StackBackendInitializer { get; set; }
        
        public Func<ICompilerBackend> NativeBackendInitializer { get; set; }

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
            else if (string.Equals(node.Name, NativeRuntimeAnnotationHandler.StackRuntimeDirectiveName,
                         StringComparison.CurrentCultureIgnoreCase))
            {
                _isNative = false;
            }
            else
            {
                _isNative = _isNativeDefault;
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