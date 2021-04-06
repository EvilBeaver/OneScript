/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Runtime;

namespace OneScript.Native.Compiler
{
    public class ModuleCompiler : ExpressionTreeGeneratorBase
    {
        private DynamicModule _module;
        
        public ModuleCompiler(IErrorSink errors) : base(errors)
        {
        }
        
        public DynamicModule Compile(
            ModuleInformation moduleInfo,
            BslSyntaxNode moduleNode,
            SymbolTable symbols
            )
        {
            InitContext(Errors, moduleInfo, symbols);
            
            _module = new DynamicModule
            {
                ModuleInformation = ModuleInfo
            };
            
            Visit(moduleNode);

            return _module;
        }

        protected override void VisitModule(ModuleNode node)
        {
            var moduleScope = new SymbolScope();
            Symbols.AddScope(moduleScope);
            base.VisitModule(node);
            Symbols.PopScope();
        }

        protected override void VisitModuleVariable(VariableDefinitionNode varNode)
        {
            var annotations = CompilerHelpers.GetAnnotations(varNode.Annotations);
            var field = new BslFieldInfo(varNode.Name);
            field.SetExportFlag(varNode.IsExported);
            field.SetAnnotations(annotations);
            
            _module.Fields.Add(field);
        }

        protected override void VisitMethod(MethodNode methodNode)
        {
            var methodCompiler = new MethodCompiler(MakeContext());
            var method = methodCompiler.CreateMethodInfo(methodNode);
            
            _module.Methods.Add(method);
        }

        protected override void VisitModuleBody(BslSyntaxNode codeBlock)
        {
            var methodCompiler = new MethodCompiler(MakeContext());
            var method = methodCompiler.CreateMethodInfo("$entry", (CodeBatchNode)codeBlock.Children[0]);
            
            _module.Methods.Add(method);
        }
    }
}