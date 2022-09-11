/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using OneScript.Compilation;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.DependencyInjection;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Runtime;
using OneScript.Sources;
using OneScript.Values;

namespace OneScript.Native.Compiler
{
    public class ModuleCompiler : ExpressionTreeGeneratorBase, ICompilerBackend
    {
        private readonly IServiceContainer _runtimeServices;
        private DynamicModule _module;

        public ModuleCompiler(IErrorSink errors, IServiceContainer runtimeServices) : base(errors)
        {
            _runtimeServices = runtimeServices;
        }
        
        public DynamicModule Compile(
            SourceCode moduleInfo,
            BslSyntaxNode moduleNode,
            SymbolTable symbols
            )
        {
            InitContext(Errors, moduleInfo, symbols);
            
            _module = new DynamicModule
            {
                Source = moduleInfo
            };
            
            Visit(moduleNode);

            return _module;
        }

        protected override BslWalkerContext MakeContext()
        {
            var c = base.MakeContext();
            c.Services = _runtimeServices;
            return c;
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
                var factory = new BslMethodInfoFactory<BslNativeMethodInfo>(() => new BslNativeMethodInfo());
                var builder = factory.NewMethod();
                VisitMethodSignature(builder, methodNode.Signature);

                var methodInfo = builder.Build();
                var symbol = new MethodSymbol
                {
                    Name = methodInfo.Name,
                    Method = methodInfo
                };
                
                Symbols.TopScope().Methods.Add(symbol, methodInfo.Name);
            }
        }

        private void VisitMethodSignature(BslMethodBuilder<BslNativeMethodInfo> builder, MethodSignatureNode node)
        {
            builder
                .Name(node.MethodName)
                .ReturnType(node.IsFunction ? typeof(BslValue): typeof(void))
                .IsExported(node.IsExported);

            foreach (var parameterNode in node.GetParameters())
            {
                CreateParameterInfo(builder.NewParameter(), parameterNode);
            }
        }

        private void CreateParameterInfo(BslParameterBuilder param, MethodParameterNode paramNode)
        {
            // TODO: Возможно, в native у нас будут все параметры как byval, даже без Знач
            param.Name(paramNode.Name);
            if(paramNode.IsByValue)
                param.ByValue(true);
            
            if(paramNode.HasDefaultValue)
                param.DefaultValue(CompilerHelpers.ValueFromLiteral(paramNode.DefaultValue));

            var attributes = CompilerHelpers.GetAnnotations(paramNode.Annotations);
            param.SetAnnotations(attributes);
        }

        protected override void VisitModule(ModuleNode node)
        {
            var moduleScope = new SymbolScope();
            Symbols.PushScope(moduleScope);
            
            RegisterLocalMethods(node);
            base.VisitModule(node);
            
            Symbols.PopScope();
        }

        protected override void VisitModuleVariable(VariableDefinitionNode varNode)
        {
            var annotations = CompilerHelpers.GetAnnotations(varNode.Annotations);
            var field = BslFieldBuilder.Create()
                .Name(varNode.Name)
                .IsExported(varNode.IsExported)
                .SetAnnotations(annotations)
                .Build();
            
            _module.Fields.Add(field);
        }

        protected override void VisitMethod(MethodNode methodNode)
        {
            var methodSymbol = Symbols.TopScope().Methods[methodNode.Signature.MethodName];
            var methodInfo = (BslNativeMethodInfo)methodSymbol.Method;

            var context = MakeContext();
            var methCompiler = new MethodCompiler(context, methodInfo);
            methCompiler.CompileMethod(methodNode);
            _module.Methods.Add(methodInfo);
        }
 
        protected override void VisitModuleBody(BslSyntaxNode moduleBody)
        {
            var factory = new BslMethodInfoFactory<BslNativeMethodInfo>(() => new BslNativeMethodInfo());
            var method = factory.NewMethod()
                .Name("$entry")
                .Build();

            _module.Methods.Add(method);
            
            var context = MakeContext();
            var methCompiler = new MethodCompiler(context, method);
            methCompiler.CompileModuleBody(moduleBody);
        }
    }
}