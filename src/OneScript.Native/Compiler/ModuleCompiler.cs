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
    public class ModuleCompiler : BslSyntaxWalker
    {
        private readonly IErrorSink _errors;
        private ModuleInformation _moduleInfo;
        private DynamicModule _module;
        private SymbolTable _ctx;

        public ModuleCompiler(IErrorSink errors)
        {
            _errors = errors;
        }
        
        public DynamicModule Compile(
            ModuleInformation moduleInfo,
            BslSyntaxNode moduleNode,
            SymbolTable symbols
            )
        {
            _moduleInfo = moduleInfo;
            _ctx = symbols;
            
            _module = new DynamicModule
            {
                ModuleInformation = _moduleInfo
            };
            
            Visit(moduleNode);

            return _module;
        }

        protected override void VisitModule(ModuleNode node)
        {
            var moduleScope = new SymbolScope();
            _ctx.AddScope(moduleScope);
            base.VisitModule(node);
            _ctx.PopScope();
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
            var methodCompiler = new MethodCompiler(_ctx, _errors, _moduleInfo);
            var method = methodCompiler.CreateMethodInfo(methodNode);
            
            _module.Methods.Add(method);
        }
    }
}