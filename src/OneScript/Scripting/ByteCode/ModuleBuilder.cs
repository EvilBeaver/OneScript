using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.ByteCode
{
    public class ModuleBuilder : IModuleBuilder
    {
        private CompilerContext _context;
        private LexemExtractor _extractor;
        private ByteCodeBlockFactory _factory;
        private ModuleImage _module;

        public ModuleBuilder(ByteCodeBlockFactory factory, CompilerContext context, LexemExtractor extractor)
        {
            this._context = context;
            this._extractor = extractor;
            this._factory = factory;
        }

        public ModuleImage Module
        {
            get { return _module; }
        }

        public CompilerContext Context
        {
            get { return _context; }
        }

        public LexemExtractor Extractor
        {
            get { return _extractor; }
        }

        public ByteCodeBlockFactory Factory
        {
            get { return _factory; }
        }

        public void Build()
        {
            _module = new ModuleImage();
            BuildModule();
            ProcessForwardedDeclarations();
        }

        private void BuildModule()
        {
            try
            {
                DispatchModuleBuild();
            }
            catch(CompilerException e)
            {
                throw;
            }
            catch(Exception e)
            {
                throw;
            }
        }

        private void ProcessForwardedDeclarations()
        {
            throw new NotImplementedException();
        }

        private void DispatchModuleBuild()
        {
            do
            {
                _extractor.Next();
                IBlockBuilder builder = null;
                switch (_extractor.LastExtractedLexem.Token)
                {
                    case Token.VarDef:
                        builder = _factory.GetVarDefinitionBuilder();
                        break;
                    case Token.Procedure:
                    case Token.Function:
                        builder = _factory.GetMethodSectionBuilder();
                        break;
                    default:
                        builder = _factory.GetCodeBatchBuilder();
                        break;                        
                }

                builder.Build();

            } while (_extractor.LastExtractedLexem.Token != Token.EndOfText);
        }

        public object GetResult()
        {
            return _module;
        }
    }
}
