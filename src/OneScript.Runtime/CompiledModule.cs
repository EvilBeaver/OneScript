using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class CompiledModule : ILoadedModule
    {
        private List<Command> _commands = new List<Command>();

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

        public IList<Command> Commands
        {
            get
            {
                return _commands;
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
    }
}
