using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Runtime.Compiler;

namespace OneScript.Runtime
{
    public class CompiledModule : ILoadedModule
    {
        private List<Command> _commands = new List<Command>();
        private ModuleVariableTable _varmap = new ModuleVariableTable();
        private ModuleConstantsTable _constants = new ModuleConstantsTable();
        private List<ModuleMethodDefinition> _methods = new List<ModuleMethodDefinition>();

        public IList<Command> Commands
        {
            get
            {
                return _commands;
            }
        }

        public ModuleVariableTable VariableTable
        {
            get { return _varmap; }
        }

        public ModuleConstantsTable Constants
        {
            get { return _constants; }
        }

        public IList<ModuleMethodDefinition> Methods
        {
            get
            {
                return _methods;
            }
        }

        #region ILoadedModule members

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ISourceCodeIndexer SourceCodeIndexer
        {
            get { throw new NotImplementedException(); }
        }

        public string EntryPointName
        {
            get;
            internal set;
        } 

        #endregion
    }
}
