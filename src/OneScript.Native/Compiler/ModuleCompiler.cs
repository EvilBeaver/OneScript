/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Runtime;
using OneScript.Values;

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

        /// <summary>
        /// Заранее заготавливаем символы методов, т.к. выражения могут ссылатся на методы,
        /// объявленные позже в теле модуля.
        /// </summary>
        /// <param name="module"></param>
        private void RegisterLocalMethods(BslSyntaxNode module)
        {
            var methodsSection = module.Children.FirstOrDefault(x => x.Kind == NodeKind.MethodsSection);
            if(methodsSection == default)
                return;
            
            foreach (var methodNode in methodsSection.Children.Cast<MethodNode>())
            {
                var methodInfo = new BslMethodInfo();
                VisitMethodSignature(methodNode.Signature);

                var symbol = new MethodSymbol
                {
                    Name = methodInfo.Name,
                    MemberInfo = methodInfo
                };
                
                Symbols.TopScope().Methods.Add(symbol, methodInfo.Name);
                
            }
        }
        
        protected override void VisitModule(ModuleNode node)
        {
            var moduleScope = new SymbolScope();
            Symbols.AddScope(moduleScope);
            
            RegisterLocalMethods(node);
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
            var methodSymbol = Symbols.TopScope().Methods[methodNode.Signature.MethodName];
            var methodInfo = (BslMethodInfo)methodSymbol.MethodInfo;

            var context = MakeContext();
            var methCompiler = new MethodCompiler(context, methodInfo);
            methCompiler.CompileMethod(methodNode);
        }
 
        protected override void VisitModuleBody(BslSyntaxNode moduleBody)
        {
            var method = new BslMethodInfo();
            method.SetName("$entry");
            
            _module.Methods.Add(method);
            
            var context = MakeContext();
            var methCompiler = new MethodCompiler(context, method);
            methCompiler.CompileModuleBody(method, moduleBody);
        }
    }
}