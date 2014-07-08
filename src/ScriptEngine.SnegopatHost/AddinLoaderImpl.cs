using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ScriptEngine.SnegopatHost
{
    [ComVisible(true)]
    [Guid("6B487933-D2BA-44A0-9310-3108B4ECDCAC")]
    public class AddinLoaderImpl : IAddinLoader
    {
        #region IAddinLoader Members

        public object load(string uri, out string fullPath, out string uniqueName, out string displayName)
        {
            throw new NotImplementedException();
        }

        public bool canUnload(string fullPath, object addin)
        {
            throw new NotImplementedException();
        }

        public bool unload(string fullPath, object addin)
        {
            throw new NotImplementedException();
        }

        public string loadCommandName()
        {
            throw new NotImplementedException();
        }

        public string selectLoadURI()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
