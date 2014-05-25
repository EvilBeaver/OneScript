using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Oscript.Interop
{
    [ComVisible(true)]
    [Guid("E8900E22-412F-45D3-9EA9-A216D416E872")]
    [ProgId("OScript")]
    [ClassInterface(ClassInterfaceType.None)]
    public class ActiveScriptImpl : IActiveScript
    {
        public ActiveScriptImpl()
        {

        }

        #region IActiveScript Members

        public void SetScriptSite(IActiveScriptSite site)
        {
            throw new NotImplementedException();
        }

        public void GetScriptSite(ref Guid riid, out IntPtr ppvObject)
        {
            throw new NotImplementedException();
        }

        public void SetScriptState(SCRIPTSTATE ss)
        {
            throw new NotImplementedException();
        }

        public void GetScriptState(out SCRIPTSTATE ss)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void AddNamedItem(string name, uint flags)
        {
            throw new NotImplementedException();
        }

        public void AddTypeLib(ref Guid rguidTypeLib, uint major, uint minor, uint flags)
        {
            throw new NotImplementedException();
        }

        public void GetScriptDispatch(string itemName, out object ppdisp)
        {
            throw new NotImplementedException();
        }

        public void GetCurrentScriptThreadiD(out uint id)
        {
            throw new NotImplementedException();
        }

        public void GetScriptThreadID(uint threadID, out uint id)
        {
            throw new NotImplementedException();
        }

        public void GetScriptThreadState(uint id, out SCRIPTTHREADSTATE state)
        {
            throw new NotImplementedException();
        }

        public void InterruptScriptThread(uint id, ref System.Runtime.InteropServices.ComTypes.EXCEPINFO info, uint flags)
        {
            throw new NotImplementedException();
        }

        public void Clone(out IActiveScript item)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
