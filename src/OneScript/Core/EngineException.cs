using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class EngineException : ApplicationException
    {
        public EngineException()
        {

        }
        public EngineException(string message) : base(message)
        {

        }

        public EngineException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }

}
