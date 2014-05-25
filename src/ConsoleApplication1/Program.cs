using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = Type.GetTypeFromCLSID(Guid.Parse("E8900E22-412F-45D3-9EA9-A216D416E872"));
            var inst = (Oscript.Interop.IActiveScript) Activator.CreateInstance(t);
            inst.Close();
        }
    }
}
