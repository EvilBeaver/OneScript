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
        private readonly BslMethodInfoFactory<BslNativeMethodInfo> _methodsFactory;

        public ModuleCompiler(IErrorSink errors, IServiceContainer runtimeServices) : base(errors)
        {
            _runtimeServices = runtimeServices;
            _methodsFactory = new BslMethodInfoFactory<BslNativeMethodInfo>(() => new BslNativeMethodInfo());
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
            
            // TODO MakeConstructor - метод инициализации и установки значения в переменную this
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
                var signature = methodNode.Signature;
                if (Symbols.FindMethod(signature.MethodName, out _))
                {
                    AddError(LocalizedErrors.DuplicateMethodDefinition(signature.MethodName), signature.Location);
                    continue;
                }

                var builder = _methodsFactory.NewMethod();
                builder.SetAnnotations(methodNode.Annotations);
                
                VisitMethodSignature(builder, methodNode.Signature);

                var methodInfo = builder.Build();
                methodInfo.IsInstance = true;
                var symbol = methodInfo.ToSymbol();
                
                Symbols.DefineMethod(symbol);
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
            RegisterLocalMethods(node);
            base.VisitModule(node);
            
            Symbols.PopScope();
        }

        protected override void VisitModuleVariable(VariableDefinitionNode varNode)
        {
            if (Symbols.FindVariable(varNode.Name, out _))
            {
                AddError(LocalizedErrors.DuplicateVarDefinition(varNode.Name));
                return;
            }

            var annotations = CompilerHelpers.GetAnnotations(varNode.Annotations).ToArray();
            var varSymbol = BslFieldBuilder.Create()
                .Name(varNode.Name)
                .IsExported(varNode.IsExported)
                .SetAnnotations(annotations)
                .ValueType(typeof(BslValue))
                .Build()
                .ToSymbol();

            var id = Symbols.GetScope(Symbols.ScopeCount - 1).DefineVariable(varSymbol);
            _module.Fields.Add(varSymbol.Field);
            
            if (varNode.IsExported)
            {
                var propertyView = BslPropertyBuilder.Create()
                    .Name(varNode.Name)
                    .IsExported(true)
                    .DeclaringType(varSymbol.Type)
                    .SetAnnotations(annotations)
                    .SetDispatchingIndex(id);
                
                _module.Properties.Add(propertyView.Build());
            }
        }

        protected override void VisitMethod(MethodNode methodNode)
        {
            var methodSymbol = Symbols.GetScope(Symbols.ScopeCount - 1).Methods[methodNode.Signature.MethodName];
            var methodInfo = (BslNativeMethodInfo)methodSymbol.Method;

            var context = MakeContext();
            var methCompiler = new MethodCompiler(context, methodInfo);
            methCompiler.CompileMethod(methodNode);
            _module.Methods.Add(methodInfo);
        }
 
        protected override void VisitModuleBody(BslSyntaxNode moduleBody)
        {
            var factory = new BslMethodInfoFactory<BslNativeMethodInfo>(() => new BslNativeMethodInfo());
            var builder = factory.NewMethod()
                .Name("$entry");
            
            var method = builder.Build();
            method.IsInstance = true;
            _module.Methods.Add(method);
            
            var context = MakeContext();
            var methCompiler = new MethodCompiler(context, method);
            methCompiler.CompileModuleBody(moduleBody);
        }
    }
}